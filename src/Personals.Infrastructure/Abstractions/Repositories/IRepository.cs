using Personals.Infrastructure.Abstractions.Entities;

namespace Personals.Infrastructure.Abstractions.Repositories;

public interface IRepository<T> where T : IEntity;