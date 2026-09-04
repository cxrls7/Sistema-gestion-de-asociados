using MemberManagementSystem.Interfaces;
using MemberManagementSystem.Models;

namespace MemberManagementSystem.Services;

/// <summary>
/// Handles all movements in the account.
/// This is where the rules for deposits, withdrawals, commissions, and balances are applied.
/// </summary>
public class MovementService : IMovementService
{
    private const decimal WithdrawalCommissionRate = 8000m;
    private const decimal LargeWithdrawalThreshold = 1000000m;

    private readonly IMovementRepository _movementRepository;
    private readonly IMemberRepository _memberRepository;

    private static string GenerateTransactionId()
    {
        return $"TX-{Guid.NewGuid():N}"[..12].ToUpperInvariant();
    }

    public MovementService(IMovementRepository movementRepository, IMemberRepository memberRepository)
    {
        _movementRepository = movementRepository;
        _memberRepository = memberRepository;
    }

    public OperationResult<Movement> RegisterDeposit(int memberId, decimal amount, string performedBy)
    {
        var member = _memberRepository.GetById(memberId);
        if (member is null)
        {
            return new OperationResult<Movement> { Success = false, Message = "El asociado no existe." };
        }

        if (amount <= 0)
        {
            return new OperationResult<Movement> { Success = false, Message = "El valor de la consignación debe ser mayor que cero." };
        }

        if (string.IsNullOrWhiteSpace(performedBy))
        {
            return new OperationResult<Movement> { Success = false, Message = "El nombre de la cajera es obligatorio." };
        }

        var movement = new Movement
        {
            TransactionId = GenerateTransactionId(),
            MemberId = memberId,
            Type = MovementType.Deposit,
            Amount = amount,
            WithdrawalCommission = 0m,
            PerformedBy = performedBy,
            CreatedAt = DateTime.Now
        };

        _movementRepository.Add(movement);

        return new OperationResult<Movement>
        {
            Success = true,
            Message = "Consignación registrada correctamente.",
            Data = movement
        };
    }

    public OperationResult<Movement> RegisterWithdrawal(int memberId, decimal amount, string performedBy)
    {
        var member = _memberRepository.GetById(memberId);
        if (member is null)
        {
            return new OperationResult<Movement> { Success = false, Message = "El asociado no existe." };
        }

        if (amount <= 0)
        {
            return new OperationResult<Movement> { Success = false, Message = "El valor del retiro debe ser mayor que cero." };
        }

        if (string.IsNullOrWhiteSpace(performedBy))
        {
            return new OperationResult<Movement> { Success = false, Message = "El nombre de la cajera es obligatorio." };
        }

        var currentBalance = GetBalance(memberId);
        decimal commission = amount > LargeWithdrawalThreshold ? WithdrawalCommissionRate : 0m;
        decimal totalWithdrawal = amount + commission;

        if (currentBalance < totalWithdrawal)
        {
            return new OperationResult<Movement>
            {
                Success = false,
                Message = $"No se puede realizar el retiro porque el saldo disponible no alcanza. Monto requerido: {totalWithdrawal:C}. Saldo disponible: {currentBalance:C}."
            };
        }

        var movement = new Movement
        {
            TransactionId = GenerateTransactionId(),
            MemberId = memberId,
            Type = MovementType.Withdrawal,
            Amount = amount,
            WithdrawalCommission = commission,
            PerformedBy = performedBy,
            CreatedAt = DateTime.Now
        };

        _movementRepository.Add(movement);

        return new OperationResult<Movement>
        {
            Success = true,
            Message = "Retiro registrado correctamente.",
            Data = movement
        };
    }

    public List<Movement> GetMovementsByMember(int memberId)
    {
        return _movementRepository.GetByMemberId(memberId);
    }

    public decimal GetBalance(int memberId)
    {
        decimal balance = 0m;

        foreach (var movement in _movementRepository.GetByMemberId(memberId))
        {
            if (movement.Type == MovementType.Deposit)
            {
                balance += movement.Amount;
            }
            else
            {
                balance -= movement.Amount + movement.WithdrawalCommission;
            }
        }

        return balance;
    }

    public decimal GetBalanceInDollars(int memberId, decimal trmValue)
    {
        if (trmValue <= 0)
        {
            return 0m;
        }

        return GetBalance(memberId) / trmValue;
    }

    public List<Movement> GetAllMovements()
    {
        return _movementRepository.GetAll();
    }

    public List<Movement> GetMovementsByDateRange(DateTime startDate, DateTime endDate)
    {
        return _movementRepository.GetByDateRange(startDate, endDate);
    }

    public List<Movement> GetTopTenLargestMovements()
    {
        return _movementRepository.GetTopTenLargest();
    }

    public List<MemberMovementSummary> GetMemberMovementSummary()
    {
        var members = _memberRepository.GetAll();
        var summaries = new List<MemberMovementSummary>();

        foreach (var member in members)
        {
            var memberMovements = _movementRepository.GetByMemberId(member.Id);
            var totalDeposits = memberMovements.Where(m => m.Type == MovementType.Deposit).Sum(m => m.Amount);
            var totalWithdrawals = memberMovements.Where(m => m.Type == MovementType.Withdrawal).Sum(m => m.Amount + m.WithdrawalCommission);

            summaries.Add(new MemberMovementSummary
            {
                MemberId = member.Id,
                MemberName = member.FullName,
                MovementCount = memberMovements.Count,
                TotalDeposits = totalDeposits,
                TotalWithdrawals = totalWithdrawals,
                CurrentBalance = totalDeposits - totalWithdrawals
            });
        }

        return summaries
            .OrderByDescending(summary => summary.MovementCount)
            .ToList();
    }
}
