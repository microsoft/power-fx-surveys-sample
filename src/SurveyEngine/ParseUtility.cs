using Microsoft.PowerFx;
using Microsoft.PowerFx.Syntax;
using Microsoft.PowerFx.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using YamlDotNet.Serialization;

namespace SurveyEngine.Parse
{
    // Helpers for converting Pocos to Models. 
    public static class ParseUtility
    {
        private static readonly Regex _simpleId = new Regex("^[A-Za-z0-9]+$");

        public static ExpressionEvaluator<T> Compile<T>(this string yamlExpression,
            Engine engine,
            ReadOnlySymbolTable symbols)
        {
            string expression = Yaml2Fx(yamlExpression);
            return new ExpressionEvaluator<T>(expression, engine, symbols);
        }

        // Decode a Yaml convention string into an fx expression.
        // String opening '=', translate literal, etc. 
        public static string Yaml2Fx(string yamlExpression)
        {
            string expression;
            // Decode raw string into a Fx expression.  
            if (string.IsNullOrWhiteSpace(yamlExpression))
            {
                expression = "Blank()";
            }
            else if (yamlExpression.StartsWith("="))
            {
                // Check for "=" sign. 
                expression = yamlExpression.Substring(1);
            }
            else
            {
                // Assume string literal. 
                expression = '"' + StrLitToken.EscapeString(yamlExpression) + '"';
            }
            return expression;
        }

        public static void VerifyApiName(this string apiName)
        {
            if (!_simpleId.IsMatch(apiName))
            {
                throw new InvalidOperationException($"Invalid ApiName: {apiName}");
            }
        }
    }
}
