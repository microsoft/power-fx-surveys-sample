using Microsoft.PowerFx;
using Microsoft.PowerFx.Types;

namespace SurveyEngine
{    
    // Implementation of survey running using Power Fx expressions.
    public class Runner : IRunner
    {
        private readonly SurveyModel _surveyModel;

        // Current question index into _surveyModel.Questions.
        private int _current;

        private readonly ReadOnlySymbolValues _answerValues;

        // per-user evaluation parameters. 
        private readonly RuntimeConfig _rc;

        public Runner(SurveyModel surveyModel, BasicUserInfo userInfo)
        {
            // All users share the same model.
            _surveyModel = surveyModel;

            // Set per-user state. 
            _answerValues = _surveyModel.AnswerSymbols.CreateValues();
            _rc = new RuntimeConfig
            {
                Values = _answerValues
            };
            _rc.SetUserInfo(userInfo);
        }


        // Evaluate Power Fx properties on a model to get a question instance.
        private async Task<Question> EvalAsync(SurveyQuestionModel question, CancellationToken cancel)
        {
            string title = await question.Title.EvalAsync(_rc, cancel);


            List<string> answers = new List<string>();
            foreach(var answer in question.Answers) 
            {
                bool isHidden = await answer.IsHidden.EvalAsync(_rc, cancel);
                if (!isHidden)
                {
                    string text = await answer.Text.EvalAsync(_rc, cancel);
                    answers.Add(text);  
                }
            }

            return new Question
            {
                Title = title,
                Answers = answers.ToArray()
            };
        }

        // Return the current question, or Null if the survey is complete.
        public async Task<Question> GetCurrentQuestionAsync(CancellationToken cancel)
        {
            while (_current < _surveyModel.Questions.Length)
            {
                SurveyQuestionModel current = _surveyModel.Questions[_current];

                bool isHidden = await current.IsHidden.EvalAsync(_rc, cancel);
                if (isHidden)
                {
                    _current++; // Skip to next
                    continue;
                }                

                Question question = await EvalAsync(current, cancel);
                return question;
            }

            return null;
        }

        // Called by host when we're done with the survey. 
        // Returns a string message calculated in OnComplete.
        public async Task<string> OnCompleteAsync(CancellationToken cancel)
        {
            // Done with survey! Run the onComplete. 
            string onCompleteExpr = await _surveyModel.OnComplete.EvalAsync(_rc, cancel);
            
            return onCompleteExpr;
        }

        // idx is index into answer array of the current question. 
        public void SetResponse(string answerText)
        {
            SurveyQuestionModel current = _surveyModel.Questions[_current];
            
            var value = FormulaValue.New(answerText);
            _answerValues.Set(current.slot, value);

            // Move to next question. 
            _current++;
        }        
    }
}