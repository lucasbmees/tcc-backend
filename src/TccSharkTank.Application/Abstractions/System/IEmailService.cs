namespace TccSharkTank.Application.Abstractions.System;

public interface IEmailService
{
    Task EnviarAsync(string para, string assunto, string corpo, CancellationToken cancellationToken);
}

public sealed class SimuladoEmailService : IEmailService
{
    public Task EnviarAsync(string para, string assunto, string corpo, CancellationToken cancellationToken)
    {
        // Simulação de envio de e-mail escrevendo no console
        Console.WriteLine($@"
======= E-MAIL SIMULADO ENVIADO =======
PARA: {para}
ASSUNTO: {assunto}
CORPO:
{corpo}
=======================================
");
        return Task.CompletedTask;
    }
}
