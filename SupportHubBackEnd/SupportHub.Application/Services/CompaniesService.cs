using CSharpFunctionalExtensions;
using SupportHub.Domain.Interfaces.Application;
using SupportHub.Domain.Interfaces.DataAccess;
using SupportHub.Domain.Models;

namespace SupportHub.Application.Services;

public class CompaniesService : ICompaniesService
{
    private readonly ICompaniesRepository _companyRepository;


    public CompaniesService(ICompaniesRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<Result<TProjectTo?>> GetByIdAsync<TProjectTo>(int companyId)
    {
        try
        {
            var company = await _companyRepository.GetByIdAsync<TProjectTo>(companyId);

            return company;
        }
        catch (Exception e)
        {
            return Result.Failure<TProjectTo>($"Error getting company by id: {e.Message}");
        }
    }

    public async Task<Result<TProjectTo?>> GetByUrlAsync<TProjectTo>(string url)
    {
        try
        {
            var company = await _companyRepository.GetByUrlAsync<TProjectTo>(url);
            return company;
        }
        catch (Exception e)
        {
            return Result.Failure<TProjectTo>($"Error getting company by id: {e.Message}");
        }
    }
}