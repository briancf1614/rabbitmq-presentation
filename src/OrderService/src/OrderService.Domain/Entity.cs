using System.Collections.Generic;

namespace OrderService.Domain
{
    public abstract class Entity
    {
        private List<object> _domainEvents;
        public IReadOnlyCollection<object> DomainEvents => _domainEvents?.AsReadOnly();

        public void AddDomainEvent(object domainEvent)
        {
            _domainEvents ??= new List<object>();
            _domainEvents.Add(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents?.Clear();
        }
    }
}
