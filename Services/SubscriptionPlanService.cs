using backend.Data;
using backend.DTOs;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public interface ISubscriptionPlanService
    {
        Task<SubscriptionPlansResponseDto> GetAllPlansAsync();
        Task<SubscriptionPlanDto?> GetPlanByIdAsync(int id);
    }

    public class SubscriptionPlanService : ISubscriptionPlanService
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionPlanService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SubscriptionPlansResponseDto> GetAllPlansAsync()
        {
            try
            {
                var plans = await _context.SubscriptionPlans
                    .Where(p => p.IsActive)
                    .Include(p => p.PlanFeatures)
                        .ThenInclude(pf => pf.Feature)
                    .OrderBy(p => p.DisplayOrder)
                    .ToListAsync();

                var planDtos = plans.Select(p => new SubscriptionPlanDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Currency = p.Currency,
                    Period = GetPeriodString(p.Period),
                    IsFeatured = p.IsFeatured,
                    Features = p.PlanFeatures?
                        .Where(pf => pf.Feature != null && pf.Feature.IsActive)
                        .OrderBy(pf => pf.Feature!.DisplayOrder)
                        .Select(pf => new PlanFeatureDto
                        {
                            Id = pf.Feature!.Id,
                            Name = pf.Feature.Name,
                            Description = pf.Feature.Description,
                            IsPremium = pf.Feature.IsPremium
                        })
                        .ToList() ?? new List<PlanFeatureDto>()
                }).ToList();

                return new SubscriptionPlansResponseDto
                {
                    Success = true,
                    Message = "Subscription plans retrieved successfully",
                    Plans = planDtos
                };
            }
            catch (Exception ex)
            {
                return new SubscriptionPlansResponseDto
                {
                    Success = false,
                    Message = $"Error retrieving plans: {ex.Message}",
                    Plans = new List<SubscriptionPlanDto>()
                };
            }
        }

        public async Task<SubscriptionPlanDto?> GetPlanByIdAsync(int id)
        {
            try
            {
                var plan = await _context.SubscriptionPlans
                    .Where(p => p.IsActive && p.Id == id)
                    .Include(p => p.PlanFeatures)
                        .ThenInclude(pf => pf.Feature)
                    .FirstOrDefaultAsync();

                if (plan == null)
                    return null;

                return new SubscriptionPlanDto
                {
                    Id = plan.Id,
                    Name = plan.Name,
                    Description = plan.Description,
                    Price = plan.Price,
                    Currency = plan.Currency,
                    Period = GetPeriodString(plan.Period),
                    IsFeatured = plan.IsFeatured,
                    Features = plan.PlanFeatures?
                        .Where(pf => pf.Feature != null && pf.Feature.IsActive)
                        .OrderBy(pf => pf.Feature!.DisplayOrder)
                        .Select(pf => new PlanFeatureDto
                        {
                            Id = pf.Feature!.Id,
                            Name = pf.Feature.Name,
                            Description = pf.Feature.Description,
                            IsPremium = pf.Feature.IsPremium
                        })
                        .ToList() ?? new List<PlanFeatureDto>()
                };
            }
            catch
            {
                return null;
            }
        }

        private static string GetPeriodString(BillingPeriod period)
        {
            return period switch
            {
                BillingPeriod.Weekly => "weekly",
                BillingPeriod.Monthly => "monthly",
                BillingPeriod.Annually => "annually",
                _ => "monthly"
            };
        }
    }
}
