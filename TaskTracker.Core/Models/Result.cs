﻿namespace TaskTracker.Core.Models
{
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public T Data { get; }
        public List<string> Errors { get; }

        private Result(bool isSuccess, T data, List<string> errors)
        {
            IsSuccess = isSuccess;
            Data = data;
            Errors = errors;
        }

        public static Result<T> Success(T data) => new(true, data, []);
        public static Result<T> Failure(List<string> errors) => new(false, default!, errors);
    }
}
