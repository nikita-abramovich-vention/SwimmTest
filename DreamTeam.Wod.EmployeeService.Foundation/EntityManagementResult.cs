using System;
using System.Collections.Generic;

namespace DreamTeam.Wod.EmployeeService.Foundation
{
    public sealed class EntityManagementResult<TEntity, TError>
    {
        public bool IsSuccessful => Errors.Count == 0;

        public TEntity Entity { get; }

        public IReadOnlyCollection<TError> Errors { get; }


        private EntityManagementResult(TEntity entity = default(TEntity), IReadOnlyCollection<TError> errors = null)
        {
            Entity = entity;
            Errors = errors ?? new List<TError>();
        }


        public static implicit operator EntityManagementResult<TEntity, TError>(TEntity entity)
        {
            return CreateSuccessful(entity);
        }

        public static implicit operator EntityManagementResult<TEntity, TError>(TError error)
        {
            return CreateUnsuccessful(new[] { error });
        }

        public static EntityManagementResult<TEntity, TError> CreateSuccessful(TEntity entity)
        {
            return new EntityManagementResult<TEntity, TError>(entity);
        }

        public static EntityManagementResult<TEntity, TError> CreateUnsuccessful(IReadOnlyCollection<TError> errors)
        {
            if (errors.Count == 0)
            {
                throw new ArgumentException("Error collection must not be empty.", nameof(errors));
            }

            return new EntityManagementResult<TEntity, TError>(errors: errors);
        }
    }
}