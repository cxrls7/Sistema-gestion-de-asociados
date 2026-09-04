using System.Globalization;
using MemberManagementSystem.Data;
using MemberManagementSystem.Dependencies;
using MemberManagementSystem.Interfaces;
using MemberManagementSystem.Models;
using MemberManagementSystem.Repositories;
using MemberManagementSystem.Services;

namespace MemberManagementSystem.UI;

/// <summary>
/// Console menu for the cooperative system.
/// It simulates a real cashier workflow with repeated operations and dollar-based balances.
/// </summary>
public class ConsoleMenu
{
    private readonly IMemberService _memberService;
    private readonly IMovementService _movementService;
    private readonly ITrmService _trmService;

    public ConsoleMenu()
    {
        MySqlDatabase.Initialize();

        var memberRepository = new MemberRepository();
        var movementRepository = new MovementRepository();

        _memberService = new MemberService(memberRepository, movementRepository);
        _movementService = new MovementService(movementRepository, memberRepository);
        _trmService = new TrmService();

        SeedDemoData();
    }

    public async Task RunAsync()
    {
        while (true)
        {
            ShowMainMenu();
            Console.Write("Seleccione una opción: ");
            string option = Console.ReadLine() ?? string.Empty;

            switch (option)
            {
                case "1":
                    RegisterMemberMenu();
                    break;
                case "2":
                    ListMembers();
                    break;
                case "3":
                    SearchMemberByDocument();
                    break;
                case "4":
                    SearchMemberByName();
                    break;
                case "5":
                    UpdateMemberMenu();
                    break;
                case "6":
                    DeleteMemberMenu();
                    break;
                case "7":
                    await ShowMemberBalanceAsync();
                    break;
                case "8":
                    await ShowMemberBalanceInDollarsAsync();
                    break;
                case "9":
                    RegisterDepositMenu();
                    break;
                case "10":
                    RegisterWithdrawalMenu();
                    break;
                case "11":
                    ShowMemberMovements();
                    break;
                case "12":
                    await ShowManagementReportsMenuAsync();
                    break;
                case "0":
                    Console.Clear();
                    Console.WriteLine("Cerrando el sistema. Gracias.");
                    return;
                default:
                    Console.WriteLine("Opción inválida. Intente nuevamente.");
                    WaitForUser();
                    break;
            }
        }
    }

    private void ShowMainMenu()
    {
        Console.Clear();
        Console.WriteLine("CAJA COOPERATIVA - PANEL DE CONTROL");
        Console.WriteLine();
        Console.WriteLine("  1.  Registrar asociado");
        Console.WriteLine("  2.  Listar asociados");
        Console.WriteLine("  3.  Buscar por documento");
        Console.WriteLine("  4.  Buscar por nombre");
        Console.WriteLine("  5.  Actualizar asociado");
        Console.WriteLine("  6.  Eliminar asociado");
        Console.WriteLine("  7.  Consultar saldo del asociado en COP");
        Console.WriteLine("  8.  Consultar el saldo de un asociado en USD");
        Console.WriteLine("  9.  Registrar consignación");
        Console.WriteLine("  10. Registrar retiro");
        Console.WriteLine("  11. Ver movimientos");
        Console.WriteLine("  12. Informes de gerencia");
        Console.WriteLine("  0.  Salir");
        Console.WriteLine();
    }

    private void RegisterMemberMenu()
    {
        while (true)
        {
            ShowSection("REGISTRAR ASOCIADO");

            string documentNumber = string.Empty;
            while (true)
            {
                Console.Write("Número de documento: ");
                documentNumber = Console.ReadLine() ?? string.Empty;

                if (!IsValidDocumentNumber(documentNumber))
                {
                    Console.WriteLine("Error: Ingrese un número de documento válido. Inténtelo nuevamente.");
                    continue;
                }

                if (_memberService.GetMemberByDocumentNumber(documentNumber) != null)
                {
                    Console.WriteLine("Error: Ya existe un asociado con ese número de documento. Inténtelo nuevamente.");
                    continue;
                }

                break;
            }

            string firstName = string.Empty;
            while (true)
            {
                Console.Write("Nombres: ");
                firstName = Console.ReadLine() ?? string.Empty;
                if (IsSimpleText(firstName))
                {
                    break;
                }

                Console.WriteLine("Error: Ingrese nombres válidos.");
            }

            string lastName = string.Empty;
            while (true)
            {
                Console.Write("Apellidos: ");
                lastName = Console.ReadLine() ?? string.Empty;
                if (IsSimpleText(lastName))
                {
                    break;
                }

                Console.WriteLine("Error: Ingrese apellidos válidos.");
            }

            string phone = string.Empty;
            while (true)
            {
                Console.Write("Teléfono: ");
                phone = Console.ReadLine() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(phone))
                {
                    break;
                }

                if (!IsValidPhone(phone))
                {
                    Console.WriteLine("Error: El teléfono solo puede tener números, espacios, + o -. Inténtelo nuevamente.");
                    continue;
                }

                bool phoneExists = _memberService.GetAllMembers()
                    .Any(m => string.Equals(m.Phone.Trim(), phone.Trim(), StringComparison.OrdinalIgnoreCase));

                if (phoneExists)
                {
                    Console.WriteLine("Error: Ya existe un asociado con ese número de teléfono. Inténtelo nuevamente.");
                    continue;
                }

                break;
            }

            string address = string.Empty;
            while (true)
            {
                Console.Write("Dirección: ");
                address = Console.ReadLine() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(address))
                {
                    break;
                }

                Console.WriteLine("Error: La dirección es obligatoria.");
            }

            OperationResult<Member> result = _memberService.RegisterMember(documentNumber, firstName, lastName, phone, address);

            if (result.Success)
            {
                Console.WriteLine($"Asociado registrado correctamente. Documento: {result.Data?.DocumentNumber}");
            }
            else
            {
                Console.WriteLine("Error: " + result.Message);
            }

            if (!AskToRepeat("registrar otro asociado")) return;
        }
    }

    private void ListMembers()
    {
        while (true)
        {
            ShowSection("LISTADO DE ASOCIADOS");

            List<Member> members = _memberService.GetAllMembers();

            if (members.Count == 0)
            {
                Console.WriteLine("No hay asociados registrados.");
                if (!AskToRepeat("consultar la lista nuevamente")) return;
                continue;
            }

            const int pageSize = 8;
            int totalPages = (int)Math.Ceiling(members.Count / (double)pageSize);
            int currentPage = 1;

            while (true)
            {
                Console.Clear();
                Console.WriteLine($"LISTADO DE ASOCIADOS  |  PÁGINA {currentPage}/{totalPages}");
                Console.WriteLine();

                int startIndex = (currentPage - 1) * pageSize;
                int endIndex = Math.Min(startIndex + pageSize, members.Count);

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(new string('-', 110));
                Console.WriteLine("| {0,-4} | {1,-12} | {2,-22} | {3,-15} | {4,-25} |", "ID", "DOCUMENTO", "NOMBRE", "TELÉFONO", "DIRECCIÓN");
                Console.WriteLine(new string('-', 110));

                for (int i = startIndex; i < endIndex; i++)
                {
                    Member member = members[i];
                    string name = member.FullName.Length > 22 ? member.FullName.Substring(0, 19) + "..." : member.FullName;
                    string address = member.Address.Length > 25 ? member.Address.Substring(0, 22) + "..." : member.Address;
                    Console.WriteLine($"| {member.Id,-4} | {member.DocumentNumber,-12} | {name,-22} | {member.Phone,-15} | {address,-25} |");
                }

                Console.WriteLine($"{new string('-', 110)}");
                Console.ResetColor();

                Console.WriteLine();
                Console.WriteLine("1. Página anterior   2. Página siguiente   0. Volver");
                Console.Write("Seleccione una opción: ");
                string pageChoice = Console.ReadLine() ?? "0";

                if (pageChoice == "1")
                {
                    if (currentPage > 1) currentPage--;
                    continue;
                }

                if (pageChoice == "2")
                {
                    if (currentPage < totalPages) currentPage++;
                    continue;
                }

                break;
            }

            if (!AskToRepeat("consultar la lista nuevamente")) return;
        }
    }

    private void SearchMemberByDocument()
    {
        while (true)
        {
            ShowSection("BUSCAR POR DOCUMENTO");
            Console.Write("Número de documento: ");
            string documentNumber = Console.ReadLine() ?? string.Empty;

            Member? member = _memberService.GetMemberByDocumentNumber(documentNumber);

            if (member == null)
            {
                Console.WriteLine("No se encontró ningún asociado con ese documento.");
            }
            else
            {
                Console.WriteLine("Documento: " + member.DocumentNumber);
                Console.WriteLine("Nombre: " + member.FullName);
                Console.WriteLine("Teléfono: " + member.Phone);
                Console.WriteLine("Dirección: " + member.Address);
            }

            if (!AskToRepeat("buscar otro asociado por documento")) return;
        }
    }

    private void SearchMemberByName()
    {
        while (true)
        {
            ShowSection("BUSCAR POR NOMBRE");
            Console.Write("Nombre o apellido: ");
            string searchText = Console.ReadLine() ?? string.Empty;

            List<Member> members = _memberService.SearchMembersByName(searchText);

            if (members.Count == 0)
            {
                Console.WriteLine("No se encontraron coincidencias.");
            }
            else
            {
                foreach (Member member in members)
                {
                    Console.WriteLine($"ID: {member.Id} | {member.DocumentNumber} | {member.FullName}");
                }
            }

            if (!AskToRepeat("buscar otro asociado por nombre")) return;
        }
    }

    private void UpdateMemberMenu()
    {
        while (true)
        {
            ShowSection("ACTUALIZAR ASOCIADO");
            Console.Write("Número de documento del asociado: ");
            string documentNumber = Console.ReadLine() ?? string.Empty;

            if (!IsValidDocumentNumber(documentNumber))
            {
                Console.WriteLine("Error: Ingrese un número de documento válido.");
                if (!AskToRepeat("actualizar otro asociado")) return;
                continue;
            }

            Member? currentMember = _memberService.GetMemberByDocumentNumber(documentNumber);
            if (currentMember == null)
            {
                Console.WriteLine("No se encontró ningún asociado con ese documento.");
                if (!AskToRepeat("actualizar otro asociado")) return;
                continue;
            }

            Console.WriteLine();
            Console.WriteLine("Asociado encontrado:");
            Console.WriteLine($"ID: {currentMember.Id}");
            Console.WriteLine($"Documento: {currentMember.DocumentNumber}");
            Console.WriteLine($"Nombre: {currentMember.FirstName}");
            Console.WriteLine($"Apellido: {currentMember.LastName}");
            Console.WriteLine($"Teléfono: {currentMember.Phone}");
            Console.WriteLine($"Dirección: {currentMember.Address}");

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("¿Qué desea actualizar?");
                Console.WriteLine("1. Documento");
                Console.WriteLine("2. Nombre");
                Console.WriteLine("3. Apellido");
                Console.WriteLine("4. Teléfono");
                Console.WriteLine("5. Dirección");
                Console.WriteLine("0. Volver");
                Console.Write("Opción: ");

                string choice = Console.ReadLine() ?? string.Empty;

                string updatedDocument = currentMember.DocumentNumber;
                string updatedFirstName = currentMember.FirstName;
                string updatedLastName = currentMember.LastName;
                string updatedPhone = currentMember.Phone;
                string updatedAddress = currentMember.Address;

                bool fieldWasValidated = true;

                switch (choice)
                {
                    case "1":
                        while (true)
                        {
                            Console.Write("Nuevo documento: ");
                            updatedDocument = Console.ReadLine() ?? string.Empty;

                            if (!IsValidDocumentNumber(updatedDocument))
                            {
                                Console.WriteLine("Error: El documento debe contener solo números. Inténtelo nuevamente.");
                                continue;
                            }

                            var existingDocumentMember = _memberService.GetMemberByDocumentNumber(updatedDocument);
                            if (existingDocumentMember != null && existingDocumentMember.Id != currentMember.Id)
                            {
                                Console.WriteLine("Error: Ya existe otro asociado con ese número de documento. Inténtelo nuevamente.");
                                continue;
                            }

                            break;
                        }
                        break;
                    case "2":
                        while (true)
                        {
                            Console.Write("Nuevo nombre: ");
                            updatedFirstName = Console.ReadLine() ?? string.Empty;
                            if (IsSimpleText(updatedFirstName))
                            {
                                break;
                            }

                            Console.WriteLine("Error: El nombre no es válido.");
                        }
                        break;
                    case "3":
                        while (true)
                        {
                            Console.Write("Nuevo apellido: ");
                            updatedLastName = Console.ReadLine() ?? string.Empty;
                            if (IsSimpleText(updatedLastName))
                            {
                                break;
                            }

                            Console.WriteLine("Error: El apellido no es válido.");
                        }
                        break;
                    case "4":
                        while (true)
                        {
                            Console.Write("Nuevo teléfono: ");
                            updatedPhone = Console.ReadLine() ?? string.Empty;

                            if (string.IsNullOrWhiteSpace(updatedPhone))
                            {
                                break;
                            }

                            if (!IsValidPhone(updatedPhone))
                            {
                                Console.WriteLine("Error: El teléfono solo puede contener números, espacios, + o -. Inténtelo nuevamente.");
                                continue;
                            }

                            bool phoneAlreadyExists = _memberService.GetAllMembers()
                                .Any(m => m.Id != currentMember.Id && string.Equals(m.Phone.Trim(), updatedPhone.Trim(), StringComparison.OrdinalIgnoreCase));

                            if (phoneAlreadyExists)
                            {
                                Console.WriteLine("Error: Ya existe otro asociado con ese número de teléfono. Inténtelo nuevamente.");
                                continue;
                            }

                            break;
                        }
                        break;
                    case "5":
                        while (true)
                        {
                            Console.Write("Nueva dirección: ");
                            updatedAddress = Console.ReadLine() ?? string.Empty;
                            if (!string.IsNullOrWhiteSpace(updatedAddress))
                            {
                                break;
                            }

                            Console.WriteLine("Error: La dirección no puede estar vacía.");
                        }
                        break;
                    case "0":
                        if (!AskToRepeat("actualizar otro asociado")) return;
                        fieldWasValidated = false;
                        break;
                    default:
                        Console.WriteLine("Opción inválida.");
                        fieldWasValidated = false;
                        continue;
                }

                if (!fieldWasValidated)
                {
                    break;
                }

                OperationResult<Member> result = _memberService.UpdateMember(
                    currentMember.Id,
                    updatedDocument,
                    updatedFirstName,
                    updatedLastName,
                    updatedPhone,
                    updatedAddress);

                Console.WriteLine(result.Success ? result.Message : "Error: " + result.Message);

                if (!result.Success)
                {
                    if (!AskToRepeat("actualizar otro dato del mismo asociado")) return;
                    continue;
                }

                Console.WriteLine();
                Console.Write("¿Desea actualizar otro dato del mismo asociado? (1 = Sí / 0 = No): ");
                string continueAnswer = Console.ReadLine() ?? "0";
                if (continueAnswer != "1")
                {
                    if (!AskToRepeat("actualizar otro asociado")) return;
                    break;
                }
            }
        }
    }

    private void DeleteMemberMenu()
    {
        while (true)
        {
            ShowSection("ELIMINAR ASOCIADO");
            Console.Write("Número de documento del asociado: ");
            string documentNumber = Console.ReadLine() ?? string.Empty;

            if (!IsValidDocumentNumber(documentNumber))
            {
                Console.WriteLine("Error: Ingrese un número de documento válido.");
                if (!AskToRepeat("eliminar otro asociado")) return;
                continue;
            }

            Member? member = _memberService.GetMemberByDocumentNumber(documentNumber);
            if (member == null)
            {
                Console.WriteLine("No se encontró ningún asociado con ese documento.");
                if (!AskToRepeat("eliminar otro asociado")) return;
                continue;
            }

            OperationResult result = _memberService.DeleteMember(member.Id);
            Console.WriteLine(result.Success ? result.Message : "Error: " + result.Message);

            if (!AskToRepeat("eliminar otro asociado")) return;
        }
    }

    private async Task ShowMemberBalanceAsync()
    {
        while (true)
        {
            ShowSection("CONSULTAR EL SALDO DE UN ASOCIADO EN PESOS COLOMBIANOS");
            Console.Write("Número de documento del asociado: ");
            string documentNumber = Console.ReadLine() ?? string.Empty;

            if (!IsValidDocumentNumber(documentNumber))
            {
                Console.WriteLine("Error: Ingrese un número de documento válido.");
                if (!AskToRepeat("consultar otro saldo")) return;
                continue;
            }

            Member? member = _memberService.GetMemberByDocumentNumber(documentNumber);
            if (member == null)
            {
                Console.WriteLine("No se encontró ningún asociado con ese documento.");
                if (!AskToRepeat("consultar otro saldo")) return;
                continue;
            }

            decimal balance = _movementService.GetBalance(member.Id);
            Console.WriteLine("Saldo actual: " + balance.ToString("C") + " COP");

            if (!AskToRepeat("consultar otro saldo")) return;
        }
    }

    private async Task ShowMemberBalanceInDollarsAsync()
    {
        while (true)
        {
            ShowSection("CONSULTAR EL SALDO DE UN ASOCIADO CONVERTIDO A DÓLARES");
            Console.Write("Número de documento del asociado: ");
            string documentNumber = Console.ReadLine() ?? string.Empty;

            if (!IsValidDocumentNumber(documentNumber))
            {
                Console.WriteLine("Error: Ingrese un número de documento válido.");
                if (!AskToRepeat("consultar otro saldo")) return;
                continue;
            }

            Member? member = _memberService.GetMemberByDocumentNumber(documentNumber);
            if (member == null)
            {
                Console.WriteLine("No se encontró ningún asociado con ese documento.");
                if (!AskToRepeat("consultar otro saldo")) return;
                continue;
            }

            TrmRate? trm = await _trmService.GetCurrentTrmAsync();
            decimal balanceInCop = _movementService.GetBalance(member.Id);

            if (trm == null || trm.Valor == null || trm.Valor.Value <= 0)
            {
                Console.WriteLine("No fue posible consultar la TRM. Se muestra el saldo en pesos colombianos.");
                Console.WriteLine("Saldo actual: " + balanceInCop.ToString("C") + " COP");
            }
            else
            {
                decimal balanceInDollars = _movementService.GetBalanceInDollars(member.Id, trm.Valor.Value);
                Console.WriteLine("Saldo actual en COP: " + balanceInCop.ToString("C") + " COP");
                Console.WriteLine("Saldo convertido a USD: " + balanceInDollars.ToString("C", new CultureInfo("en-US")) + " USD");
                Console.WriteLine("TRM: " + trm.Valor.Value.ToString("N2"));
            }

            if (!AskToRepeat("consultar otro saldo")) return;
        }
    }

    private void RegisterDepositMenu()
    {
        while (true)
        {
            ShowSection("REGISTRAR CONSIGNACIÓN");

            string documentNumber = string.Empty;
            while (true)
            {
                Console.Write("Número de documento del asociado: ");
                documentNumber = Console.ReadLine() ?? string.Empty;
                Member? member = _memberService.GetMemberByDocumentNumber(documentNumber);

                if (IsValidDocumentNumber(documentNumber) && member != null)
                {
                    break;
                }

                if (!IsValidDocumentNumber(documentNumber))
                {
                    Console.WriteLine("Error: Ingrese un número de documento válido.");
                    continue;
                }

                Console.WriteLine("No se encontró ningún asociado con ese documento.");
            }

            decimal amount;
            while (true)
            {
                Console.Write("Valor: ");
                string amountText = Console.ReadLine() ?? string.Empty;
                if (IsValidPositiveAmount(amountText, out amount))
                {
                    break;
                }

                Console.WriteLine("Error: Ingrese un valor válido mayor que cero.");
            }

            string cashier = string.Empty;
            while (true)
            {
                Console.Write("Nombre de la cajera: ");
                cashier = Console.ReadLine() ?? string.Empty;
                if (IsSimpleText(cashier))
                {
                    break;
                }

                Console.WriteLine("Error: Ingrese un nombre de cajera válido.");
            }

            OperationResult<Movement> result = _movementService.RegisterDeposit(_memberService.GetMemberByDocumentNumber(documentNumber)!.Id, amount, cashier);
            Console.WriteLine(result.Success ? result.Message : "Error: " + result.Message);

            if (!AskToRepeat("registrar otra consignación")) return;
        }
    }

    private void RegisterWithdrawalMenu()
    {
        while (true)
        {
            ShowSection("REGISTRAR RETIRO");

            string documentNumber = string.Empty;
            Member? member = null;
            while (true)
            {
                Console.Write("Número de documento del asociado: ");
                documentNumber = Console.ReadLine() ?? string.Empty;
                member = _memberService.GetMemberByDocumentNumber(documentNumber);

                if (IsValidDocumentNumber(documentNumber) && member != null)
                {
                    break;
                }

                if (!IsValidDocumentNumber(documentNumber))
                {
                    Console.WriteLine("Error: Ingrese un número de documento válido.");
                    continue;
                }

                Console.WriteLine("No se encontró ningún asociado con ese documento.");
            }

            decimal amount;
            while (true)
            {
                Console.Write("Valor: ");
                string amountText = Console.ReadLine() ?? string.Empty;
                if (IsValidPositiveAmount(amountText, out amount))
                {
                    break;
                }

                Console.WriteLine("Error: Ingrese un valor válido mayor que cero.");
            }

            string cashier = string.Empty;
            while (true)
            {
                Console.Write("Nombre de la cajera: ");
                cashier = Console.ReadLine() ?? string.Empty;
                if (IsSimpleText(cashier))
                {
                    break;
                }

                Console.WriteLine("Error: Ingrese un nombre de cajera válido.");
            }

            OperationResult<Movement> result = _movementService.RegisterWithdrawal(member!.Id, amount, cashier);
            Console.WriteLine(result.Success ? result.Message : "Error: " + result.Message);

            if (!AskToRepeat("registrar otro retiro")) return;
        }
    }

    private void ShowMemberMovements()
    {
        while (true)
        {
            ShowSection("MOVIMIENTOS DEL ASOCIADO");
            Console.Write("Número de documento del asociado: ");
            string documentNumber = Console.ReadLine() ?? string.Empty;

            if (!IsValidDocumentNumber(documentNumber))
            {
                Console.WriteLine("Error: Ingrese un número de documento válido.");
                if (!AskToRepeat("consultar otro movimiento")) return;
                continue;
            }

            Member? member = _memberService.GetMemberByDocumentNumber(documentNumber);
            if (member == null)
            {
                Console.WriteLine("No se encontró ningún asociado con ese documento.");
                if (!AskToRepeat("consultar otro movimiento")) return;
                continue;
            }

            List<Movement> movements = _movementService.GetMovementsByMember(member.Id);

            if (movements.Count == 0)
            {
                Console.WriteLine("Este asociado no tiene movimientos registrados.");
            }
            else
            {
                const int pageSize = 6;
                int totalPages = (int)Math.Ceiling(movements.Count / (double)pageSize);
                int currentPage = 1;

                while (true)
                {
                    Console.Clear();
                    Console.WriteLine($"HISTORIAL DE MOVIMIENTOS | PÁGINA {currentPage}/{totalPages}");
                    Console.WriteLine($"Asociado: {member.FullName} | Documento: {member.DocumentNumber}");
                    Console.WriteLine();
                    Console.WriteLine(new string('-', 118));
                    Console.WriteLine("| {0,-15} | {1,-10} | {2,-12} | {3,-16} | {4,-14} | {5,-18} |", "ID TRANSACCIÓN", "FECHA", "TIPO", "VALOR", "COMISIÓN", "CAJERA");
                    Console.WriteLine(new string('-', 118));

                    int startIndex = (currentPage - 1) * pageSize;
                    int endIndex = Math.Min(startIndex + pageSize, movements.Count);

                    for (int i = startIndex; i < endIndex; i++)
                    {
                        Movement movement = movements[i];
                        string tipoMovimiento = movement.Type == MovementType.Deposit ? "Consignación" : "Retiro";
                        Console.WriteLine($"| {movement.TransactionId,-15} | {movement.CreatedAt:dd/MM/yyyy,-10} | {tipoMovimiento,-12} | {movement.Amount:C,-16} | {movement.WithdrawalCommission:C,-14} | {movement.PerformedBy,-18} |");
                    }

                    Console.WriteLine($"{new string('-', 118)}");
                    Console.WriteLine();
                    Console.WriteLine("1. Página anterior   2. Página siguiente   0. Volver");
                    Console.Write("Seleccione una opción: ");
                    string pageChoice = Console.ReadLine() ?? "0";

                    if (pageChoice == "1")
                    {
                        if (currentPage > 1) currentPage--;
                        continue;
                    }

                    if (pageChoice == "2")
                    {
                        if (currentPage < totalPages) currentPage++;
                        continue;
                    }

                    break;
                }
            }

            if (!AskToRepeat("consultar otro movimiento")) return;
        }
    }

    private async Task ShowManagementReportsMenuAsync()
    {
        while (true)
        {
            ShowSection("INFORMES DE GERENCIA");
            Console.WriteLine("1. ¿Cuánta plata tenemos?");
            Console.WriteLine("2. ¿Quiénes son mis mejores asociados?");
            Console.WriteLine("3. ¿Quiénes están dormidos?");
            Console.WriteLine("4. ¿Cómo nos fue en un período?");
            Console.WriteLine("5. ¿Cuáles fueron los movimientos más grandes?");
            Console.WriteLine("6. ¿Quién me está moviendo la caja?");
            Console.WriteLine("0. Regresar");
            Console.Write("Opción: ");

            string option = Console.ReadLine() ?? string.Empty;

            switch (option)
            {
                case "1":
                    await ShowTotalBalanceReportAsync();
                    break;
                case "2":
                    ShowTopMembersReport();
                    break;
                case "3":
                    ShowDormantMembersReport();
                    break;
                case "4":
                    ShowPeriodReport();
                    break;
                case "5":
                    ShowLargestMovementsReport();
                    break;
                case "6":
                    ShowActiveMembersReport();
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("Opción inválida.");
                    if (!AskToRepeat("consultar otro informe")) return;
                    break;
            }

            if (option != "0" && !AskToRepeat("consultar otro informe")) return;
        }
    }

    private async Task ShowTotalBalanceReportAsync()
    {
        ShowSection("¿CUÁNTA PLATA TENEMOS?");

        List<Member> members = _memberService.GetAllMembers();
        decimal totalBalance = 0m;

        foreach (Member member in members)
        {
            totalBalance += _movementService.GetBalance(member.Id);
        }

        TrmRate? trm = await _trmService.GetCurrentTrmAsync();
        decimal totalBalanceInDollars = trm != null && trm.Valor != null && trm.Valor.Value > 0
            ? totalBalance / trm.Valor.Value
            : totalBalance;

        decimal averageBalance = members.Count == 0 ? 0 : totalBalance / members.Count;

        Console.WriteLine("Saldo total de la cooperativa: " + totalBalance.ToString("C") + " COP");
        Console.WriteLine("Equivalente en USD: " + totalBalanceInDollars.ToString("F2") + " USD");
        Console.WriteLine("Cantidad de asociados: " + members.Count);
        Console.WriteLine("Saldo promedio por asociado: " + averageBalance.ToString("C") + " COP");
    }

    private void ShowTopMembersReport()
    {
        ShowSection("¿QUIÉNES SON MIS MEJORES ASOCIADOS?");

        List<Member> members = _memberService.GetAllMembers();
        List<Member> sortedMembers = members.OrderByDescending(m => _movementService.GetBalance(m.Id)).Take(5).ToList();

        foreach (Member member in sortedMembers)
        {
            Console.WriteLine($"{member.DocumentNumber} | {member.FullName} | {_movementService.GetBalance(member.Id):C} COP");
        }

        Console.WriteLine("Nota: la conversión a USD se realiza como referencia secundaria.");
    }

    private void ShowDormantMembersReport()
    {
        ShowSection("¿QUIÉNES ESTÁN DORMIDOS?");

        List<Member> members = _memberService.GetAllMembers();
        bool found = false;

        foreach (Member member in members)
        {
            if (_movementService.GetMovementsByMember(member.Id).Count == 0)
            {
                Console.WriteLine($"{member.DocumentNumber} | {member.FullName} | Registrado: {member.RegistrationDate:dd/MM/yyyy}");
                found = true;
            }
        }

        if (!found)
        {
            Console.WriteLine("No hay asociados dormidos.");
        }
    }

    private void ShowPeriodReport()
    {
        ShowSection("¿CÓMO NOS FUE EN UN PERÍODO?");
        Console.Write("Fecha inicial (dd/MM/yyyy): ");
        string startText = Console.ReadLine() ?? string.Empty;
        Console.Write("Fecha final (dd/MM/yyyy): ");
        string endText = Console.ReadLine() ?? string.Empty;

        if (!DateTime.TryParse(startText, out DateTime startDate) || !DateTime.TryParse(endText, out DateTime endDate))
        {
            Console.WriteLine("Las fechas no son válidas.");
            return;
        }

        List<Movement> movements = _movementService.GetMovementsByDateRange(startDate, endDate);

        decimal totalDeposits = 0m;
        decimal totalWithdrawals = 0m;

        foreach (Movement movement in movements)
        {
            if (movement.Type == MovementType.Deposit)
            {
                totalDeposits += movement.Amount;
            }
            else
            {
                totalWithdrawals += movement.Amount + movement.WithdrawalCommission;
            }
        }

        Console.WriteLine("Consignaciones: " + totalDeposits.ToString("C") + " COP");
        Console.WriteLine("Retiros: " + totalWithdrawals.ToString("C") + " COP");
        Console.WriteLine("Diferencia: " + (totalDeposits - totalWithdrawals).ToString("C") + " COP");
        Console.WriteLine("Movimientos de consignación: " + movements.Count(m => m.Type == MovementType.Deposit));
        Console.WriteLine("Movimientos de retiro: " + movements.Count(m => m.Type == MovementType.Withdrawal));
    }

    private void ShowLargestMovementsReport()
    {
        ShowSection("¿CUÁLES FUERON LOS MOVIMIENTOS MÁS GRANDES?");

        List<Movement> movements = _movementService.GetTopTenLargestMovements();

        foreach (Movement movement in movements)
        {
            Member? member = _memberService.GetAllMembers().FirstOrDefault(m => m.Id == movement.MemberId);
            string memberName = member != null ? member.FullName : "Desconocido";
            string tipoMovimiento = movement.Type == MovementType.Deposit ? "Consignación" : "Retiro";
            Console.WriteLine($"{movement.CreatedAt:dd/MM/yyyy} | {tipoMovimiento} | {movement.Amount:C} COP | {memberName}");
        }
    }

    private void ShowActiveMembersReport()
    {
        ShowSection("¿QUIÉN ME ESTÁ MOVIENDO LA CAJA?");

        List<MemberMovementSummary> summaries = _movementService.GetMemberMovementSummary();

        foreach (MemberMovementSummary summary in summaries)
        {
            Console.WriteLine($"{summary.MemberName} | Movimientos: {summary.MovementCount} | Consignado: {summary.TotalDeposits:C} COP | Retirado: {summary.TotalWithdrawals:C} COP | Saldo: {summary.CurrentBalance:C} COP");
        }
    }

    private void SeedDemoData()
    {
        if (_memberService.GetAllMembers().Count > 0)
        {
            return;
        }

        OperationResult<Member> member1 = _memberService.RegisterMember("1001", "Ana", "Garcia", "3001234567", "Street 10");
        OperationResult<Member> member2 = _memberService.RegisterMember("1002", "Luis", "Perez", "3017654321", "Street 20");
        OperationResult<Member> member3 = _memberService.RegisterMember("1003", "Camila", "Rojas", "3029988777", "Avenue 30");

        if (member1.Success && member1.Data != null)
        {
            _movementService.RegisterDeposit(member1.Data.Id, 5000000m, "Cajera A");
            _movementService.RegisterWithdrawal(member1.Data.Id, 1000000m, "Cajera B");
        }

        if (member2.Success && member2.Data != null)
        {
            _movementService.RegisterDeposit(member2.Data.Id, 3000000m, "Cajera C");
        }

        if (member3.Success && member3.Data != null)
        {
            _movementService.RegisterDeposit(member3.Data.Id, 7500000m, "Cajera D");
            _movementService.RegisterWithdrawal(member3.Data.Id, 200000m, "Cajera E");
        }
    }

    private void ShowSection(string title)
    {
        Console.Clear();
        Console.WriteLine(title);
        Console.WriteLine();
    }

    private static bool IsSimpleText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        foreach (char character in value)
        {
            if (!char.IsLetter(character) && !char.IsWhiteSpace(character) && character != '-' && character != '.')
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsValidDocumentNumber(string value)
    {
        return !string.IsNullOrWhiteSpace(value) && value.All(char.IsDigit);
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

    private static bool IsValidPositiveAmount(string value, out decimal amount)
    {
        return decimal.TryParse(value, out amount) && amount > 0;
    }

    private static bool AskToRepeat(string actionText)
    {
        Console.WriteLine();
        Console.Write($"¿Desea {actionText}? (1 = Sí / 0 = Volver al menú): ");
        string answer = Console.ReadLine() ?? "0";
        return answer == "1";
    }

    private static void WaitForUser()
    {
        Console.WriteLine();
        Console.Write("Presione cualquier tecla para continuar...");
        Console.ReadKey();
    }
}
