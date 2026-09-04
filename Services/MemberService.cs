using MemberManagementSystem.Interfaces;
using MemberManagementSystem.Models;

namespace MemberManagementSystem.Services;

/// <summary>
/// Handles the operations related to members.
/// Here we apply the validation rules and the business logic:
/// unique documents, search by name, update of data, and deletion restrictions.
/// </summary>
public class MemberService : IMemberService
{
    private readonly IMemberRepository _memberRepository;
    private readonly IMovementRepository _movementRepository;

    public MemberService(IMemberRepository memberRepository, IMovementRepository movementRepository)
    {
        _memberRepository = memberRepository;
        _movementRepository = movementRepository;
    }

    public OperationResult<Member> RegisterMember(string documentNumber, string firstName, string lastName, string phone, string address)
    {
        if (string.IsNullOrWhiteSpace(documentNumber) || !IsValidDocumentNumber(documentNumber))
        {
            return new OperationResult<Member> { Success = false, Message = "Ingrese un número de documento válido." };
        }

        if (string.IsNullOrWhiteSpace(firstName) || !IsValidName(firstName))
        {
            return new OperationResult<Member> { Success = false, Message = "Ingrese un nombre válido." };
        }

        if (string.IsNullOrWhiteSpace(lastName) || !IsValidName(lastName))
        {
            return new OperationResult<Member> { Success = false, Message = "Ingrese un apellido válido." };
        }

        if (!string.IsNullOrWhiteSpace(phone) && !IsValidPhone(phone))
        {
            return new OperationResult<Member> { Success = false, Message = "Ingrese un teléfono válido." };
        }

        if (string.IsNullOrWhiteSpace(address))
        {
            return new OperationResult<Member> { Success = false, Message = "Ingrese una dirección válida." };
        }

        if (_memberRepository.ExistsByDocumentNumber(documentNumber))
        {
            return new OperationResult<Member> { Success = false, Message = "Ya existe un asociado con ese número de documento." };
        }

        if (!string.IsNullOrWhiteSpace(phone) && _memberRepository.ExistsByPhone(phone))
        {
            return new OperationResult<Member> { Success = false, Message = "Ya existe un asociado con ese número de teléfono." };
        }

        var member = new Member
        {
            DocumentNumber = documentNumber.Trim(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Phone = phone.Trim(),
            Address = address.Trim(),
            RegistrationDate = DateTime.Now
        };

        _memberRepository.Add(member);

        return new OperationResult<Member>
        {
            Success = true,
            Message = "Asociado registrado correctamente.",
            Data = member
        };
    }

    public List<Member> GetAllMembers()
    {
        return _memberRepository.GetAll();
    }

    public Member? GetMemberByDocumentNumber(string documentNumber)
    {
        return _memberRepository.GetByDocumentNumber(documentNumber);
    }

    public List<Member> SearchMembersByName(string name)
    {
        return _memberRepository.SearchByName(name);
    }

    public OperationResult<Member> UpdateMember(int id, string documentNumber, string firstName, string lastName, string phone, string address)
    {
        var member = _memberRepository.GetById(id);
        if (member is null)
        {
            return new OperationResult<Member> { Success = false, Message = "No se encontró el asociado." };
        }

        if (string.IsNullOrWhiteSpace(documentNumber) || !IsValidDocumentNumber(documentNumber))
        {
            return new OperationResult<Member> { Success = false, Message = "Ingrese un número de documento válido." };
        }

        if (string.IsNullOrWhiteSpace(firstName) || !IsValidName(firstName))
        {
            return new OperationResult<Member> { Success = false, Message = "Ingrese un nombre válido." };
        }

        if (string.IsNullOrWhiteSpace(lastName) || !IsValidName(lastName))
        {
            return new OperationResult<Member> { Success = false, Message = "Ingrese un apellido válido." };
        }

        if (!string.IsNullOrWhiteSpace(phone) && !IsValidPhone(phone))
        {
            return new OperationResult<Member> { Success = false, Message = "Ingrese un teléfono válido." };
        }

        if (string.IsNullOrWhiteSpace(address))
        {
            return new OperationResult<Member> { Success = false, Message = "Ingrese una dirección válida." };
        }

        if (_memberRepository.ExistsByDocumentNumber(documentNumber, id))
        {
            return new OperationResult<Member> { Success = false, Message = "Otro asociado ya tiene ese número de documento." };
        }

        if (!string.IsNullOrWhiteSpace(phone) && _memberRepository.ExistsByPhone(phone, id))
        {
            return new OperationResult<Member> { Success = false, Message = "Otro asociado ya tiene ese número de teléfono." };
        }

        member.DocumentNumber = documentNumber.Trim();
        member.FirstName = firstName.Trim();
        member.LastName = lastName.Trim();
        member.Phone = phone.Trim();
        member.Address = address.Trim();

        _memberRepository.Update(member);

        return new OperationResult<Member>
        {
            Success = true,
            Message = "Asociado actualizado correctamente.",
            Data = member
        };
    }

    public OperationResult DeleteMember(int id)
    {
        var member = _memberRepository.GetById(id);
        if (member is null)
        {
            return new OperationResult { Success = false, Message = "No se encontró el asociado." };
        }

        bool hasMovements = _movementRepository.GetByMemberId(id).Any();
        bool hasBalance = GetBalance(id) != 0m;

        if (hasMovements || hasBalance)
        {
            return new OperationResult
            {
                Success = false,
                Message = "No se puede eliminar el asociado porque tiene movimientos o un saldo distinto de cero."
            };
        }

        _memberRepository.Delete(id);
        return new OperationResult { Success = true, Message = "Asociado eliminado correctamente." };
    }

    public decimal GetBalance(int memberId)
    {
        var movements = _movementRepository.GetByMemberId(memberId);
        decimal balance = 0m;

        foreach (var movement in movements)
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

    private static bool IsValidName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        foreach (char character in value)
        {
            if (char.IsDigit(character))
            {
                return false;
            }

            if (!char.IsLetter(character) && !char.IsWhiteSpace(character) && character != '-' && character != '.')
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsValidDocumentNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        foreach (char character in value)
        {
            if (!char.IsDigit(character))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsValidPhone(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        foreach (char character in value)
        {
            if (!char.IsDigit(character) && !char.IsWhiteSpace(character) && character != '-' && character != '+')
            {
                return false;
            }
        }

        return true;
    }
}
