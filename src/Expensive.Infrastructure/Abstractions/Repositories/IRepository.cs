using Expensive.Infrastructure.Abstractions.Entities;

namespace Expensive.Infrastructure.Abstractions.Repositories;

public interface IRepository<T> where T : IEntity;