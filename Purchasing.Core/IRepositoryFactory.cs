﻿using Purchasing.Core.Domain;
using UCDArch.Core.PersistanceSupport;

namespace Purchasing.Core
{
    public interface IRepositoryFactory
    {
        IRepositoryWithTypedId<Account, string> AccountRepository { get; set; }
        IRepository<Approval> ApprovalRepository { get; set; }
        IRepository<AutoApproval> AutoApprovalRepository { get; set; }
        IRepository<ConditionalApproval> ConditionalApprovalRepository { get; set; }
        IRepository<Order> OrderRepository { get; set; }
        IRepository<OrderStatusCode> OrderStatusCodeRepository { get; set; }
        IRepositoryWithTypedId<User, string> UserRepository { get; set; }
        IRepository<Workgroup> WorkgroupRepository { get; set; }
        IRepository<WorkgroupPermission> WorkgroupPermissionRepository { get; set; }
        IRepository<WorkgroupAccount> WorkgroupAccountRepository { get; set; }
    }

    public class RepositoryFactory : IRepositoryFactory
    {
        public IRepositoryWithTypedId<Account, string> AccountRepository { get; set; }
        public IRepository<Approval> ApprovalRepository { get; set; }
        public IRepository<AutoApproval> AutoApprovalRepository { get; set; }
        public IRepository<ConditionalApproval> ConditionalApprovalRepository { get; set; }
        public IRepository<Order> OrderRepository { get; set; }
        public IRepository<OrderStatusCode> OrderStatusCodeRepository { get; set; }
        public IRepositoryWithTypedId<User, string> UserRepository { get; set; }
        public IRepository<WorkgroupAccount> WorkgroupAccountRepository { get; set; }
        public IRepository<WorkgroupPermission> WorkgroupPermissionRepository { get; set; }
        public IRepository<Workgroup> WorkgroupRepository { get; set; }
    }
}