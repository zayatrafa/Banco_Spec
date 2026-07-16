Console.WriteLine("Bem vindo ao Banco Spec!");

Cliente cliente = new Cliente();

cliente.Nome = "Joao da Silva";
cliente.CPF = "123.456.789-00";
cliente.Email = "joao.silva@email.com";

ContaCorrente contaCorrente = new ContaCorrente(cliente, 500);

contaCorrente.TransacaoRealizada += transacao =>
{
    Console.WriteLine($"EVENTO CONTA CORRENTE: {transacao.Tipo} de {transacao.Valor} para {transacao.Titular.Nome}");
};

contaCorrente.Depositar(1000);

Console.WriteLine($"Cliente: {contaCorrente.Titular.Nome}");
Console.WriteLine($"CPF: {contaCorrente.Titular.CPF}");
Console.WriteLine($"Email: {contaCorrente.Titular.Email}");
Console.WriteLine($"Saldo inicial: {contaCorrente.Saldo}");
Console.WriteLine($"Limite cheque especial: {contaCorrente.LimiteChequeEspecial}");

contaCorrente.Sacar(200);

Console.WriteLine($"Saldo após saque: {contaCorrente.Saldo}");

ContaPoupanca contaPoupanca = new ContaPoupanca(cliente, 0.01m);

contaPoupanca.TransacaoRealizada += transacao =>
{
    Console.WriteLine($"EVENTO POUPANCA: {transacao.Tipo} de {transacao.Valor} para {transacao.Titular.Nome}");
};

contaPoupanca.Depositar(1000);
contaPoupanca.AplicarRendimento();

Console.WriteLine($"Saldo poupança: {contaPoupanca.Saldo}");

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
    Console.WriteLine($"Saldo após saque polimórfico: {conta.Saldo}");
    Console.WriteLine();
}

try {
    ExecutarCenarioComBug(contaCorrente);
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Entrada inválida: {ex.Message}");
}
catch (SaldoInsuficienteException ex)
{
    Console.WriteLine($"Erro: {ex.Message}");
}

Console.WriteLine();
Console.WriteLine("Exemplo de injecao de dependencia:");

// INotificador notificador = new EmailNotificador();
INotificador notificador = new SmsNotificador();

ServicoBancario servicoBancario = new ServicoBancario(notificador);

Console.WriteLine("Exemplo simples de async/await:");

Task envioSms = EnviarSmsDemoradoAsync();
Task sistemaTrabalhando = SistemaContinuaTrabalhandoAsync();

await Task.WhenAll(envioSms, sistemaTrabalhando);

Console.WriteLine("As duas tarefas terminaram.");

Console.WriteLine();
Console.WriteLine("Exemplo de async/await dentro do servico bancario:");
Console.WriteLine("Antes de iniciar o deposito assincrono.");

Task depositoComNotificacao = servicoBancario.RealizarDepositoAsync(contaCorrente, 150, "Bonus recebido");

Console.WriteLine("O sistema continuou executando enquanto a notificacao esta sendo enviada.");
Console.WriteLine($"Saldo consultado imediatamente apos iniciar a operacao: {contaCorrente.Saldo}");

for (int i = 1; i <= 5; i++)
{
    Console.WriteLine($"Sistema ainda responsivo... {i}");
    await Task.Delay(1000);
}

Console.WriteLine("Agora vamos aguardar a conclusao da notificacao antes de encerrar o programa.");

await depositoComNotificacao;

Console.WriteLine("Deposito e notificacao finalizados.");

static async Task EnviarSmsDemoradoAsync()
{
    Console.WriteLine("SMS: comecei a enviar. Vou demorar 5 segundos.");

    await Task.Delay(5000);

    Console.WriteLine("SMS: terminei de enviar.");
}

static async Task SistemaContinuaTrabalhandoAsync()
{
    for (int i = 1; i <= 5; i++)
    {
        Console.WriteLine($"SISTEMA: continuei trabalhando enquanto o SMS nao terminou. Passo {i}");
        await Task.Delay(1000);
    }
}

static void ExecutarCenarioComBug(ContaCorrente contaCorrente)
{
    Console.WriteLine("Executando cenario com bug...");

    decimal valorSaque = 0;

    contaCorrente.Sacar(valorSaque);

    Console.WriteLine($"Saque de {valorSaque} realizado com sucesso.");
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

    public event TransacaoRealizadaHandler? TransacaoRealizada;

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
        Depositar(valor, "Depósito sem descrição");
    }

    public void Depositar(decimal valor, string descricao)
    {
        if (valor <= 0)
        {
            throw new ArgumentException("Valor inválido para depósito.", nameof(valor));
        }

        saldo += valor;

        Console.WriteLine($"Depósito realizado: {descricao}");
        NotificarTransacaoRealizada("Deposito", valor);
    }

    public virtual void Sacar(decimal valor)
    {
        if (valor <= 0)
        {
            throw new ArgumentException("Valor inválido para saque.");
        }

        if (valor > saldo)
        {
            throw new SaldoInsuficienteException("Saldo insuficiente.");
        }

        Debitar(valor);
        NotificarTransacaoRealizada("Saque", valor);
    }

    protected void Debitar(decimal valor)
    {
        saldo -= valor;
    }

    protected void NotificarTransacaoRealizada(string tipo, decimal valor)
    {
        TransacaoRealizada?.Invoke(new TransacaoEventArgs(Titular, tipo, valor, Saldo));
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
            throw new ArgumentException("Valor inválido para saque.");
        }

        if (valor > Saldo + LimiteChequeEspecial)
        {
            throw new SaldoInsuficienteException("Saldo insuficiente, mesmo considerando o limite do cheque especial.");
        }

        Debitar(valor);
        NotificarTransacaoRealizada("Saque", valor);
    }
}

public delegate void TransacaoRealizadaHandler(TransacaoEventArgs transacao);

public class TransacaoEventArgs
{
    public Cliente Titular { get; }
    public string Tipo { get; }
    public decimal Valor { get; }
    public decimal SaldoAtual { get; }

    public TransacaoEventArgs(Cliente titular, string tipo, decimal valor, decimal saldoAtual)
    {
        Titular = titular;
        Tipo = tipo;
        Valor = valor;
        SaldoAtual = saldoAtual;
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

public class SaldoInsuficienteException : Exception
{
    public SaldoInsuficienteException(string mensagem) : base(mensagem)
    {
    }
}

public interface INotificador
{
    Task EnviarMensagemAsync(Cliente cliente, string mensagem);
}

public class EmailNotificador : INotificador
{
    public async Task EnviarMensagemAsync(Cliente cliente, string mensagem)
    {
        Console.WriteLine("Preparando envio de email...");
        await Task.Delay(5000);
        Console.WriteLine($"Email enviado para {cliente.Email}: {mensagem}");
    }
}

public class SmsNotificador : INotificador
{
    public async Task EnviarMensagemAsync(Cliente cliente, string mensagem)
    {
        Console.WriteLine("Preparando envio de SMS...");
        await Task.Delay(5000);
        Console.WriteLine($"SMS enviado para {cliente.Nome}: {mensagem}");
    }
}

public class ServicoBancario
{
    private readonly INotificador notificador;

    public ServicoBancario(INotificador notificador)
    {
        this.notificador = notificador;
    }

    public async Task RealizarDepositoAsync(ContaBancaria conta, decimal valor, string descricao)
    {
        conta.Depositar(valor, descricao);

        Console.WriteLine("Deposito concluido. Aguardando envio da notificacao...");

        await notificador.EnviarMensagemAsync(
            conta.Titular,
            $"Deposito de {valor} realizado com sucesso."
        );

        Console.WriteLine("Servico bancario terminou o fluxo de deposito.");
    }
}
