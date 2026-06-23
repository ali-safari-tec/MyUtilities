using System.Reflection;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace MyUtilities.DataBase.Sql
{
    public class SqlDataBaseServises<TEntity, TKey> : IDataBaseServices<TEntity, TKey> where TEntity : class
    {
        protected readonly DbContext _context;
        protected readonly DbSet<TEntity> _dbSet;
        protected readonly ILogger _logger;
        private readonly PropertyInfo _keyProperty;

        public SqlDataBaseServises(DbContext context, ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<TEntity>();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _keyProperty = DetectKeyProperty();
        }

        private PropertyInfo DetectKeyProperty()
        {
            var entityType = _context.Model.FindEntityType(typeof(TEntity));
            var key = entityType?.FindPrimaryKey();
            var property = key?.Properties.FirstOrDefault()?.PropertyInfo;

            return property is null ? throw new InvalidOperationException($"کلید اصلی برای {typeof(TEntity).Name} پیدا نشد.") : property;
        }

        private Expression<Func<TEntity, bool>> BuildKeyPredicate(TKey id)
        {
            var parameter = Expression.Parameter(typeof(TEntity), "e");
            var property = Expression.Property(parameter, _keyProperty);
            var constant = Expression.Constant(id, typeof(TKey));
            var equality = Expression.Equal(property, constant);
            return Expression.Lambda<Func<TEntity, bool>>(equality, parameter);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            var entityName = typeof(TEntity).Name;

            try
            {
                var result = await _dbSet.ToListAsync();
                _logger.LogInformation("Successfully retrieved all {Count} entities of type {EntityName}.", result.Count, entityName);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while retrieving all entities of type {EntityName}.", entityName);
                throw;
            }
        }

        public async Task<TEntity?> GetByIdAsync(TKey id)
        {
            ArgumentNullException.ThrowIfNull(id);

            var entityName = typeof(TEntity).Name;

            try
            {
                var entity = await _dbSet.FirstOrDefaultAsync(BuildKeyPredicate(id));

                if (entity is null)
                {
                    _logger.LogWarning("Entity of type {EntityName} with ID {EntityId} not found.", entityName, id);
                    return null;
                }

                _logger.LogInformation("Entity of type {EntityName} with ID {EntityId} found successfully.", entityName, id);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while finding entity of type {EntityName} with ID {EntityId}.", entityName, id);
                throw;
            }
        }

        public async Task<IEnumerable<TEntity>> GetByIdsAsync(IEnumerable<TKey> ids)
        {
            ArgumentNullException.ThrowIfNull(ids);

            var entityName = typeof(TEntity).Name;
            var idList = ids.ToList();

            if (idList.Count == 0)
            {
                _logger.LogWarning("Received an empty list of IDs for {EntityName}. No entities will be retrieved.", entityName);
                return [];
            }

            try
            {
                var entities = await _dbSet.Where(e => idList.Contains((TKey)_keyProperty.GetValue(e)!)).ToListAsync();

                if (!entities.Any())
                {
                    _logger.LogWarning("No entities of type {EntityName} found with the provided IDs: {EntityIds}.", entityName, string.Join(", ", idList));
                    return [];
                }

                _logger.LogInformation("Found {Count} entities of type {EntityName} with IDs: {EntityIds}.", entities.Count, entityName, string.Join(", ", idList));
                return entities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while finding {EntityName} with IDs: {EntityIds}.", entityName, string.Join(", ", idList));
                throw;
            }
        }

        public async Task<bool> ExistsAsync(TKey id)
        {
            ArgumentNullException.ThrowIfNull(id);

            try
            {
                var exists = await _dbSet.AnyAsync(BuildKeyPredicate(id));

                if (exists)
                {
                    _logger.LogInformation("Entity with ID {EntityId} exist.", id);
                    return true;
                }

                _logger.LogWarning("Entity with ID {EntityId} dont exist.", id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while searching entity with ID {EntityId}.", id);
                throw;
            }
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> predicate)
        {
            ArgumentNullException.ThrowIfNull(predicate);

            var entityName = typeof(TEntity).Name;

            try
            {
                var find = await _dbSet.Where(predicate).ToListAsync();

                if (find.Any())
                {
                    _logger.LogInformation("Found {Count} entities of type {EntityName} matching the predicate.", find.Count, entityName);
                    return find;
                }
                else
                {
                    _logger.LogWarning("No entities of type {EntityName} found matching the provided predicate.", entityName);
                    return [];
                }
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error occurred while finding entities of type {EntityName} with the provided predicate.", entityName);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while finding entities of type {EntityName} with the provided predicate.", entityName);
                throw;
            }
        }

        public async Task<TEntity> AddAsync(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            try
            {
                await _dbSet.AddAsync(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Entity {EntityName} added successfully.", typeof(TEntity).Name);

                return entity;
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error occurred while adding entity {EntityName}.", typeof(TEntity).Name);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding entity {EntityName}.", typeof(TEntity).Name);
                throw;
            }
        }

        public async Task<(bool Success, int Count, IEnumerable<TEntity>)> AddRangeAsync(IEnumerable<TEntity> entities)
        {
            ArgumentNullException.ThrowIfNull(entities);

            var entityName = typeof(TEntity).Name;
            var items = entities.ToList();

            try
            {

                await _dbSet.AddRangeAsync(items);
                int count = await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully added {Count} entities of type {EntityName}.", count, entityName);
                return (true, count, items);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error occurred while adding multiple {EntityName} entities.", entityName);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while adding multiple {EntityName} entities.", entityName);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(TKey id)
        {
            ArgumentNullException.ThrowIfNull(id);

            var entityName = typeof(TEntity).Name;

            try
            {
                var delete = await _dbSet.Where(BuildKeyPredicate(id)).ExecuteDeleteAsync();

                if (delete > 0)
                {
                    _logger.LogInformation("Entity of type {EntityName} with ID {EntityId} deleted successfully.", entityName, id);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Entity of type {EntityName} with ID {EntityId} not found for deletion.", entityName, id);
                    return false;
                }
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error occurred while deleting entity of type {EntityName} with ID {EntityId}.", entityName, id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while deleting entity of type {EntityName} with ID {EntityId}.", entityName, id);
                throw;
            }
        }

        public async Task<(bool Success, int Count)> DeleteRangeAsync(Expression<Func<TEntity, bool>> predicate)
        {
            ArgumentNullException.ThrowIfNull(predicate);

            var entityName = typeof(TEntity).Name;

            try
            {
                var delete = await _dbSet.Where(predicate).ExecuteDeleteAsync();

                if (delete > 0)
                {
                    _logger.LogInformation("Successfully deleted {DeletedCount} entities of type {EntityName} using the provided predicate.", delete, entityName);
                    return (true, delete);
                }
                else
                {
                    _logger.LogWarning("No entities of type {EntityName} found matching the provided predicate. No records were deleted.", entityName);
                    return (false, 0);
                }
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error occurred while bulk deleting entities of type {EntityName} with the provided predicate.", entityName);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during bulk deletion of {EntityName} entities with the provided predicate.", entityName);
                throw;
            }
        }

        public async Task<TEntity?> UpdateAsync(TKey id, TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(id);
            ArgumentNullException.ThrowIfNull(entity);

            var entityName = typeof(TEntity).Name;

            try
            {
                var existingEntity = await _dbSet.FirstOrDefaultAsync(BuildKeyPredicate(id));
                if (existingEntity is null)
                {
                    _logger.LogWarning("Entity of type {EntityName} with ID matching predicate not found. Cannot update.", entityName);
                    return null;
                }

                _context.Entry(existingEntity).CurrentValues.SetValues(entity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Entity of type {EntityName} with ID matching predicate updated successfully.", entityName);
                return existingEntity;

            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error occurred while updating Entity of type {EntityName} with ID {EntityId}.", entityName, id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating Entity of type {EntityName} with ID {EntityId}.", entityName, id);
                throw;
            }
        }

        public async Task<TEntity?> UpdateAsync(TKey id, Action<TEntity> updateAction)
        {
            ArgumentNullException.ThrowIfNull(id);
            ArgumentNullException.ThrowIfNull(updateAction);

            var entityName = typeof(TEntity).Name;

            try
            {
                var entityToUpdate = await _dbSet.FirstOrDefaultAsync(BuildKeyPredicate(id));

                if (entityToUpdate is null)
                {
                    _logger.LogWarning("Entity of type {EntityName} with ID matching predicate not found. Cannot update.", entityName);
                    return null;
                }

                updateAction(entityToUpdate);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Entity of type {EntityName} with ID matching predicate updated successfully.", entityName);
                return entityToUpdate;
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error occurred while updating entity of type {EntityName} with ID matching predicate.", entityName);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating entity of type {EntityName} with ID matching predicate.", entityName);
                throw;
            }
        }

        public async Task<(bool Success, int Count, IEnumerable<TEntity>)> UpdateRangeAsync(IEnumerable<(TKey id, TEntity entity)> updates)
        {
            ArgumentNullException.ThrowIfNull(updates);

            var entityName = typeof(TEntity).Name;
            int successfulUpdateCount = 0;
            var updatedEntities = new List<TEntity>();

            var updateData = updates.ToList();

            if (updateData.Count == 0)
            {
                _logger.LogWarning("Received an empty list of updates for {EntityName}. No records were updated.", entityName);
                return (false, 0, new List<TEntity>());
            }

            foreach (var updateItem in updateData)
            {
                var id = updateItem.id;
                var entityData = updateItem.entity;

                try
                {
                    var existingEntity = await _dbSet.FirstOrDefaultAsync(BuildKeyPredicate(id));

                    if (existingEntity is null)
                    {
                        _logger.LogWarning("Entity of type {EntityName} with ID matching predicate {Predicate} not found. Skipping update.", entityName, BuildKeyPredicate(id).ToString());
                        continue;
                    }

                    _context.Entry(existingEntity).CurrentValues.SetValues(entityData);

                    successfulUpdateCount++;
                    updatedEntities.Add(existingEntity);
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, "Database error occurred while updating entity of type {EntityName} with ID matching predicate {Predicate}.", entityName, BuildKeyPredicate(id).ToString());
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unexpected error occurred while updating entity of type {EntityName} with ID matching predicate {Predicate}.", entityName, BuildKeyPredicate(id).ToString());
                    throw;
                }
            }

            if (successfulUpdateCount > 0)
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully updated {Count} entities of type {EntityName}.", successfulUpdateCount, entityName);
                return (true, successfulUpdateCount, updatedEntities);
            }
            else
            {
                _logger.LogWarning("No entities of type {EntityName} were successfully updated.", entityName);
                return (false, 0, new List<TEntity>());
            }
        }

        public async Task<int> CountAsync()
        {
            try
            {
                var result = await _dbSet.CountAsync();
                _logger.LogInformation("Counted all entities of type {EntityName}. Count: {Count}", typeof(TEntity).Name, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during CountAsync for entity {EntityName}.", typeof(TEntity).Name);
                throw;
            }
        }

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> filter)
        {
            ArgumentNullException.ThrowIfNull(filter);

            try
            {
                var result = await _dbSet.CountAsync(filter);
                _logger.LogInformation("Counted filtered entities of type {EntityName}. Count: {Count}", typeof(TEntity).Name, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during CountAsync(filter) for entity {EntityName}.", typeof(TEntity).Name);
                throw;
            }
        }
    }
}
