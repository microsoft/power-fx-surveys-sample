using Microsoft.PowerFx;
using Microsoft.PowerFx.Types;
using SurveyEngine.Parse;
using YamlDotNet.Serialization;

namespace SurveyEngine
{
    // POCO classes represent raw model from the .yaml file.     
    public class SurveyPoco
    {
        public SurveyQuestionPoco[] Questions { get; set; }

        // Expression to run when survey is completed. 
        // This should save results to a data source.
        public string OnComplete { get; set; }

        public static SurveyPoco ParseYmlFile(string path)
        {
            var contents = File.ReadAllText(path);

            var deserializer = new DeserializerBuilder()
                .Build();

            var poco = deserializer.Deserialize<SurveyPoco>(contents);

            return poco;
        }

        public SurveyModel Resolve(RecalcEngine engine)
        {
            SymbolTable symbols = new SymbolTable
            {
                DebugName = "Answers"
            };

            return new SurveyModel
            {
                Questions = Array.ConvertAll(this.Questions, x => x.Resolve(engine, symbols)),
                AnswerSymbols = symbols,
                OnComplete = this.OnComplete.Compile<string>(engine, symbols)
            };
        }
    }

    public class SurveyQuestionPoco
    {
        public string Title { get; set; }
        public string ApiName { get; set; }

        // If present, Fx expression that must evaluate to true. 
        public string IsHidden { get; set; }

        public SurveyAnswerPoco[] Answers { get; set; }

        // Resolve expressions against current symbols
        // and then add this answer to the symbol table for next question.
        public SurveyQuestionModel Resolve(RecalcEngine engine, SymbolTable symbols)
        {
            var title = this.Title.Compile<string>(engine, symbols);
            var isVisible = this.IsHidden.Compile<bool>(engine, symbols);
            var answers = Array.ConvertAll(this.Answers, x => x.Resolve(engine, symbols));

            this.ApiName.VerifyApiName();
            ISymbolSlot slot = symbols.AddVariable(this.ApiName, FormulaType.String);

            return new SurveyQuestionModel
            {
                Title = title,
                IsHidden = isVisible,
                Answers = answers,
                slot = slot
            };
        }
    }

    public class SurveyAnswerPoco
    {
        public string Text { get; set; }

        public string IsHidden { get; set; }

        public SurveyAnswerModel Resolve(RecalcEngine engine, ReadOnlySymbolTable symbols)
        {
            return new SurveyAnswerModel
            {
                Text = this.Text.Compile<string>(engine, symbols),
                IsHidden = this.IsHidden.Compile<bool>(engine, symbols),
            };
        }
    }

}


