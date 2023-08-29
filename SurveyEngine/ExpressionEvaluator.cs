// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Microsoft.PowerFx;
using Microsoft.PowerFx.Syntax;
using Microsoft.PowerFx.Types;

namespace SurveyEngine
{
    // Helper to parse a class and hold onto a type-safe IExpressionEvaluator.
    public class ExpressionEvaluator<T>
    {
        private readonly IExpressionEvaluator _inner;

        public ExpressionEvaluator(
            string expression,
            Engine engine,
            ReadOnlySymbolTable symbols
            )
        {
            // Check for errors, but don't run yet. 
            var check = new CheckResult(engine)
                .SetText(expression)
                .SetBindingInfo(symbols);

            if (PrimitiveValueConversions.TryGetFormulaType(typeof(T),
                out FormulaType expectedType))
            {
                check.SetExpectedReturnValue(expectedType);
            }

            check.ApplyErrors();

            // Get errors, could show in UI. 
            if (!check.IsSuccess)
            {
                var errors = string.Join(",",
                    check.Errors.Select(error => error.ToString()));
                var msg = $"Failed to compile: {expression}\r\n" + errors;
                throw new InvalidOperationException(msg);
            }

            _inner = check.GetEvaluator();
        }

        public ExpressionEvaluator(IExpressionEvaluator inner)
        {
            _inner = inner;
        }

        public async Task<T> EvalAsync(RuntimeConfig runtime, CancellationToken cancel)
        {
            FormulaValue result = await _inner.EvalAsync(cancel, runtime);

            if (result is PrimitiveValue<T> str)
            {
                return str.Value;
            }

            if (result is BlankValue blank)
            {
                return default(T);
            }

            // Runtime errors, like divide-by-zero. 
            // Ok to case because we must either be T, Blank, or Error. 
            var error = (ErrorValue)result;

            var errorList = error.Errors.Select(x => x.ToString());
            var msg = string.Join(",", errorList);
            throw new InvalidOperationException(msg);
        }
    }
}
