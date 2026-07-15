Console.WriteLine("Bem vindo ao Banco Spec!");

Cliente cliente = new Cliente();

cliente.Nome = "Joao da Silva";
cliente.CPF = "123.456.789-00";
cliente.Email = "joao.silva@email.com";

ContaCorrente contaCorrente = new ContaCorrente(cliente, 500);

contaCorrente.Depositar(1000);

Console.WriteLine($"Cliente: {contaCorrente.Titular.Nome}");
Console.WriteLine($"CPF: {contaCorrente.Titular.CPF}");
Console.WriteLine($"Email: {contaCorrente.Titular.Email}");
Console.WriteLine($"Saldo inicial: {contaCorrente.Saldo}");
Console.WriteLine($"Limite cheque especial: {contaCorrente.LimiteChequeEspecial}");

contaCorrente.Sacar(200);

Console.WriteLine($"Saldo após saque: {contaCorrente.Saldo}");

ContaPoupanca contaPoupanca = new ContaPoupanca(cliente, 0.01m);

contaPoupanca.Depositar(1000);
contaPoupanca.AplicarRendimento();

Console.WriteLine($"Saldo poupanca: {contaPoupanca.Saldo}");

List<ContaBancaria> contas = new List<ContaBancaria>();

contas.Add(contaCorrente);
contas.Add(contaPoupanca);

Console.WriteLine();
Console.WriteLine("Resumo das contas:");

foreach (ContaBancaria conta in contas)
{
    Console.WriteLine($"Titular: {conta.Titular.Nome}");
    Console.WriteLine($"Saldo: {conta.Saldo}");
    conta.Sacar(50);
    Console.WriteLine($"Saldo apos saque polimorfico: {conta.Saldo}");
    Console.WriteLine();
}

public class Cliente
{
    public string Nome { get; set; } = string.Empty;
    public string CPF { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public abstract class ContaBancaria
{
    private decimal saldo;

    public Cliente Titular { get; }

    public decimal Saldo
    {
        get { return saldo; }
    }

    public ContaBancaria(Cliente titular)
    {
        Titular = titular;
    }

    public void Depositar(decimal valor)
    {
        if (valor <= 0)
        {
            Console.WriteLine("Valor invalido para deposito.");
            return;
        }

        saldo += valor;
    }

    public virtual void Sacar(decimal valor)
    {
        if (valor <= 0)
        {
            Console.WriteLine("Valor invalido para saque.");
            return;
        }

        if (valor > saldo)
        {
            Console.WriteLine("Saldo insuficiente.");
            return;
        }

        Debitar(valor);
    }

    protected void Debitar(decimal valor)
    {
        saldo -= valor;
    }
}

public class ContaCorrente : ContaBancaria
{
    public decimal LimiteChequeEspecial { get; set; }

    public ContaCorrente(Cliente titular, decimal limiteChequeEspecial) : base(titular)
    {
        LimiteChequeEspecial = limiteChequeEspecial;
    }

    public override void Sacar(decimal valor)
    {
        if (valor <= 0)
        {
            Console.WriteLine("Valor invalido para saque.");
            return;
        }

        if (valor > Saldo + LimiteChequeEspecial)
        {
            Console.WriteLine("Saldo insuficiente, mesmo considerando o limite do cheque especial.");
            return;
        }

        Debitar(valor);
    }
}

public class ContaPoupanca : ContaBancaria
{
    public decimal TaxaRendimento { get; set; }

    public ContaPoupanca(Cliente titular, decimal taxaRendimento) : base(titular)
    {
        TaxaRendimento = taxaRendimento;
    }

    public void AplicarRendimento()
    {
        decimal rendimento = Saldo * TaxaRendimento;

        Depositar(rendimento);
    }
}