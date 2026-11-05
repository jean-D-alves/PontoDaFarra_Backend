using Microsoft.EntityFrameworkCore;
using Npgsql;
using PontoDaFarra.context;
using PontoDaFarra.Exceptions;
using PontoDaFarra.models;
using PontoDaFarra.repositories.Interface;

namespace PontoDaFarra.repositories
{
    public class LegalDocumentRepository(AppDbContext context) : ILegalDocumentRepository
    {
        public async Task<LegalDocument?> GetActiveByTypeAsync(LegalDocumentType type)
        {
            return await context.LegalDocuments
                .AsNoTracking()
                .FirstOrDefaultAsync(document => document.Type == type && document.IsActive);
        }

        public async Task<List<LegalDocument>> GetActiveAsync()
        {
            return await context.LegalDocuments
                .AsNoTracking()
                .Where(document => document.IsActive)
                .OrderBy(document => document.Type)
                .ToListAsync();
        }

        public async Task<List<LegalDocument>> GetAllAsync()
        {
            return await context.LegalDocuments
                .AsNoTracking()
                .OrderBy(document => document.Type)
                .ThenByDescending(document => document.PublishedAt)
                .ToListAsync();
        }

        public async Task<LegalDocument?> GetByIdAsync(Guid legalDocumentId)
        {
            return await context.LegalDocuments
                .FirstOrDefaultAsync(document => document.LegalDocumentId == legalDocumentId);
        }

        public async Task<List<LegalDocument>> GetByIdsAsync(IReadOnlyCollection<Guid> legalDocumentIds)
        {
            return await context.LegalDocuments
                .AsNoTracking()
                .Where(document => legalDocumentIds.Contains(document.LegalDocumentId))
                .ToListAsync();
        }

        public async Task<bool> VersionExistsAsync(
            LegalDocumentType type,
            string version,
            Guid? excludeLegalDocumentId = null)
        {
            var query = context.LegalDocuments
                .AsNoTracking()
                .Where(document => document.Type == type && document.Version == version);

            if (excludeLegalDocumentId.HasValue)
                query = query.Where(document => document.LegalDocumentId != excludeLegalDocumentId.Value);

            return await query.AnyAsync();
        }

        public async Task<LegalDocument> PublishAsync(LegalDocument document)
        {
            await using var transaction = await context.Database.BeginTransactionAsync();

            var activeDocuments = await context.LegalDocuments
                .Where(current => current.Type == document.Type && current.IsActive)
                .ToListAsync();

            foreach (var active in activeDocuments)
                active.IsActive = false;

            await context.SaveChangesAsync();

            context.LegalDocuments.Add(document);

            try
            {
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateException exception) when (exception.InnerException is PostgresException
                { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                throw new ConflictException("A legal document with this version already exists for this type");
            }

            return document;
        }

        public async Task<LegalDocument> UpdateAsync(LegalDocument document)
        {
            context.LegalDocuments.Update(document);

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException exception) when (exception.InnerException is PostgresException
                { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                throw new ConflictException("A legal document with this version already exists for this type");
            }

            return document;
        }
    }
}
