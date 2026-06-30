using System;
using System.Collections.Generic;

namespace CozyComfort.API.Models.DTOs
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResponse<T> SuccessResult(T data, string message = "")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> ErrorResult(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }

    public class PaginatedResponse<T>
    {
        public List<T> Data { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }

        public static PaginatedResponse<T> Create(List<T> data, int pageNumber, int pageSize, int totalCount)
        {
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            return new PaginatedResponse<T>
            {
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasPreviousPage = pageNumber > 1,
                HasNextPage = pageNumber < totalPages
            };
        }
    }

    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public string? Detail { get; set; }
        public int StatusCode { get; set; }
        public string? TraceId { get; set; }
        public Dictionary<string, List<string>> ValidationErrors { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class DashboardStatsResponse
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int AwaitingProductionOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int TotalSellers { get; set; }
        public int TotalDistributors { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<OrderStatusCount> OrdersByStatus { get; set; } = new();
        public List<DailyOrderCount> DailyOrders { get; set; } = new();
        public List<DistributorPerformanceSummary> DistributorPerformance { get; set; } = new();
    }

    public class OrderStatusCount
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class DailyOrderCount
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public decimal Revenue { get; set; }
    }

    public class DistributorPerformanceSummary
    {
        public string DistributorName { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal SuccessRate { get; set; }
        public int AverageResponseTime { get; set; }
    }
}
