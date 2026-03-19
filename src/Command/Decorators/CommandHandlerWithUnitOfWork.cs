using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.Domain;

namespace SFA.DAS.Learning.Command.Decorators
{
    public class CommandHandlerWithUnitOfWork<T> : ICommandHandler<T> where T : ICommand
    {
        private readonly ICommandHandler<T> _handler;
        private readonly LearningDataContext _dataContext;
        private readonly IUnitOfWork _unitOfWork;

        public CommandHandlerWithUnitOfWork(
            ICommandHandler<T> handler,
            LearningDataContext dataContext,
            IUnitOfWork unitOfWork)
        {
            _handler = handler;
            _dataContext = dataContext;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(T command, CancellationToken cancellationToken = default)
        {
            var transaction = await _dataContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await _handler.Handle(command, cancellationToken);
                await _unitOfWork.CommitAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }

    public class CommandHandlerWithUnitOfWork<T, TResult> : ICommandHandler<T, TResult> where T : ICommand
    {
        private readonly ICommandHandler<T, TResult> _handler;
        private readonly LearningDataContext _dataContext;
        private readonly IUnitOfWork _unitOfWork;

        public CommandHandlerWithUnitOfWork(
            ICommandHandler<T, TResult> handler,
            LearningDataContext dataContext,
            IUnitOfWork unitOfWork)
        {
            _handler = handler;
            _dataContext = dataContext;
            _unitOfWork = unitOfWork;
        }

        public async Task<TResult> Handle(T command, CancellationToken cancellationToken = default)
        {
            var transaction = await _dataContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var result = await _handler.Handle(command, cancellationToken);
                await _unitOfWork.CommitAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
