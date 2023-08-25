namespace SurveyEngine
{
    // Interface for single instance of survey.
    public interface IRunner
    {
        // Get the current question. This is a title and array of answers.
        Task<Question> GetCurrentQuestionAsync(CancellationToken cancel);

        // Set answer text for current question and move to next question. 
        void SetResponse(string answerText);

        // Called at end of survey. 
        // Returns final message to display
        Task<string> OnCompleteAsync(CancellationToken cancel);
    }

    // Instance of a question. 
    // This has all expressions evaluated and is ready for consumption.
    public class Question
    {
        public string Title { get; set; }
        public string[] Answers { get; set; }

    }
}