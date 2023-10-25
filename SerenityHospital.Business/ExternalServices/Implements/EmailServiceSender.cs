﻿using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using NETCore.MailKit.Core;
using NETCore.MailKit.Infrastructure.Internal;
using SerenityHospital.Business.ExternalServices.Interfaces;
using SerenityHospital.Core.Entities;

namespace SerenityHospital.Business.ExternalServices.Implements;

public class EmailServiceSender : IEmailServiceSender
{
    private readonly EmailConfiguration _emailConfig;

    public EmailServiceSender(EmailConfiguration emailConfig)
    {
        _emailConfig = emailConfig;
    }

    public void SendEmail(Message message)
    {
        var emailMessage = CreateEmailMessage(message);
        Send(emailMessage);
    }

    private MimeMessage CreateEmailMessage(Message message)
    {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress("email", _emailConfig.From));
        emailMessage.To.AddRange(message.To);
        emailMessage.Subject = message.Subject;
        emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = message.Content };

        return emailMessage;
    }

    private void Send(MimeMessage mailMessage)
    {
        using var client = new SmtpClient();
        try
        {
            client.Connect(_emailConfig.SmtpServer, _emailConfig.Port, SecureSocketOptions.StartTls);
            client.AuthenticationMechanisms.Remove("XOAUTH2");
            client.Authenticate(_emailConfig.UserName, _emailConfig.Password);

            client.Send(mailMessage);
        }
        catch
        {
            throw;
        }
        finally
        {
            client.Disconnect(true);
            client.Dispose();
        }
    }
}
