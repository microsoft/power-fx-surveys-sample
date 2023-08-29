// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Microsoft.PowerFx;

namespace SurveyEngine
{
    // Models are compiled from the Poco. 
    // These can be evaluated many times across multiple users. 
    public class SurveyModel
    {
        public SurveyQuestionModel[] Questions { get; init; }

        /// <summary>
        /// Symbol table where answers are stored. 
        /// </summary>
        public ReadOnlySymbolTable AnswerSymbols { get;init; }

        public ExpressionEvaluator<string> OnComplete { get; init; }
    }

    public class SurveyQuestionModel
    {
        public ExpressionEvaluator<string> Title { get; init; }

        /// <summary>
        /// The slot of the variable to write answer.
        /// </summary>
        public ISymbolSlot slot { get; init; }

        // Only show question if true. 
        public ExpressionEvaluator<bool> IsHidden { get; init; }

        public SurveyAnswerModel[] Answers { get; init; }
    }

    public class SurveyAnswerModel
    {
        // Evaluates to a string. 
        public ExpressionEvaluator<string> Text { get; init; }

        // Only show question if true. 
        public ExpressionEvaluator<bool> IsHidden { get; init; }
    }
}
