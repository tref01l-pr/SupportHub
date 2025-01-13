using AutoMapper;
using AutoMapper.QueryableExtensions;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using SupportHub.DataAccess.SqlServer.Entities;
using SupportHub.Domain.Interfaces.DataAccess;
using SupportHub.Domain.Models;

namespace SupportHub.DataAccess.SqlServer.Repositories;

public class CompaniesRepository : ICompaniesRepository
{
    private readonly SupportHubDbContext _context;
    private readonly IMapper _mapper;
    private IConfigurationProvider _mapperConfig => _mapper.ConfigurationProvider;

    public CompaniesRepository(SupportHubDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<TProjectTo?> GetByIdAsync<TProjectTo>(int companyId) =>
        await _context.Companies
            .AsNoTracking()
            .Where(company => company.Id == companyId)
            .ProjectTo<TProjectTo>(_mapperConfig)
            .FirstOrDefaultAsync();

    public async Task<TProjectTo?> GetByUrlAsync<TProjectTo>(string url) =>
        await _context.Companies
            .AsNoTracking()
            .Where(company => company.Url == url)
            .ProjectTo<TProjectTo>(_mapperConfig)
            .FirstOrDefaultAsync();

    public async Task<Result<Company>> CreateAsync(Company company)
    {
        var companyEntity = _mapper.Map<Company, CompanyEntity>(company);
        await _context.Companies.AddAsync(companyEntity);
        var result = await SaveAsync();
        if (!result.Value)
        {
            return Result.Failure<Company>("Something went wrong during save message");
        }

        return _mapper.Map<CompanyEntity, Company>(companyEntity);
    }

    public async Task<Result> DeleteAsync(int companyId)
    {
        var companyEntity = await _context.Companies
            .Where(c => c.Id == companyId)
            .FirstOrDefaultAsync();

        if (companyEntity == null)
        {
            return Result.Failure<bool>("Company not found");
        }

        _context.Companies.Remove(companyEntity);
        var result = await SaveAsync();
        return result.Value
            ? Result.Success()
            : Result.Failure($"Something went wrong during deletion of {typeof(CompanyEntity)}!");
    }

    public async Task<Result<bool>> SaveAsync() => await _context.SaveChangesAsync() > 0;
}