namespace Medior.BaseTypes
{
    public class Result
    {
        public static Result Ok()
        {
            return new Result(true);
        }
        public static Result Fail(string error)
        {
            return new Result(false, error);
        }

        public static Result<T> Ok<T>(T value)
        {
            return new Result<T>(true, value, null);
        }

        public static Result<T> Fail<T>(string error)
        {
            return new Result<T>(false, default, error);
        }

        public Result(bool isSuccess, string? error = null)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public bool IsSuccess { get; init; }

        public string? Error { get; init; }
    }

    public class Result<T>
    {
        public Result(bool isSuccess, T? value, string? error)
        {
            IsSuccess = isSuccess;
            Error = error;
            Value = value;
        }

        public bool IsSuccess { get; init; }

        public string? Error { get; init; }

        public T? Value { get; init; }
    }
}
