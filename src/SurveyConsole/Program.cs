using Microsoft.PowerFx;
using SurveyEngine;

namespace SurveyConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var survey = SurveyPoco.ParseYmlFile(@".\Samples\Basic2.yml");

            RecalcEngine fxEngine = new RecalcEngine();
            fxEngine.Config.SymbolTable.AddUserInfoObject(
                nameof(UserInfo.FullName), nameof(UserInfo.Email));

            var model = survey.Resolve(fxEngine);

            var userInfo = new BasicUserInfo
            {
                Email = "john@contoso.com", FullName = "John Smith"                
            };
            var runner = new Runner(model, userInfo);

            Console.WriteLine($">> Running Survey for {userInfo.FullName}");

            // Drive questions 
            while(true)
            {
                Console.WriteLine();

                var question = runner.GetCurrentQuestionAsync(default).Result;
                if (question == null)
                {
                    Console.WriteLine("Survey complete!");
                    var finalMessage = runner.OnCompleteAsync(default).Result;
                    Console.WriteLine(finalMessage);

                    return;
                }
                Console.WriteLine(question.Title);

                int i = 0;
                foreach(var answer in question.Answers)
                {
                    Console.WriteLine($"{i}. {answer}");
                    i++;
                }

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"Enter answer #: ");
                var x = int.Parse(Console.ReadLine());
                Console.ResetColor();

                var answerText = question.Answers[x];
                runner.SetResponse(answerText);
                
            }
        }
    }
}