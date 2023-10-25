﻿using SerenityHospital.Business.Contracts;
using SerenityHospital.Core.Entities.Stripe;
using Stripe;

namespace SerenityHospital.Business.Application;

public class StripeAppService : IStripeAppService
{
    private readonly ChargeService _chargeService;
    private readonly CustomerService _customerService;
    private readonly TokenService _tokenService;

    public StripeAppService(
        ChargeService chargeService,
        CustomerService customerService,
        TokenService tokenService)
    {
        _chargeService = chargeService;
        _customerService = customerService;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Create a new customer at Stripe through API using customer and card details from records.
    /// </summary>
    /// <param name="customer">Stripe Customer</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns>Stripe Customer</returns>
    public async Task<StripeCustomer> AddStripeCustomerAsync(AddStripeCustomer customer, CancellationToken ct)
    {
        TokenCreateOptions tokenOptions = new TokenCreateOptions
        {
            Card = new TokenCardOptions
            {
                Name = customer.Name,
                Number = customer.CreditCard.CardNumber,
                ExpYear = customer.CreditCard.ExpirationYear,
                ExpMonth = customer.CreditCard.ExpirationMonth,
                Cvc = customer.CreditCard.Cvc
            }
        };

        Token stripeToken = await _tokenService.CreateAsync(tokenOptions, null, ct);

        CustomerCreateOptions customerOptions = new CustomerCreateOptions
        {
            Name = customer.Name,
            Email = customer.Email,
            Source = stripeToken.Id
        };

        Customer createdCustomer = await _customerService.CreateAsync(customerOptions, null, ct);

        return new StripeCustomer(createdCustomer.Name, createdCustomer.Email, createdCustomer.Id);
        //Set Customer options - We need to create the customer first as we will need the id
    }

    /// <summary>
    /// Add a new payment at Stripe using Customer and Payment details.
    /// Customer has to exist at Stripe already.
    /// </summary>
    /// <param name="payment">Stripe Payment</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns><Stripe Payment/returns>
    public async Task<StripePayment> AddStripePaymentAsync(AddStripePayment payment, CancellationToken ct)
    {
        // Set the options for the payment we would like to create at Stripe
        ChargeCreateOptions paymentOptions = new ChargeCreateOptions
        {
            Customer = payment.CustomerId,
            ReceiptEmail = payment.ReceiptEmail,
            Description = payment.Description,
            Currency = payment.Currency,
            Amount = payment.Amount
        };

        // Create the payment
        var createdPayment = await _chargeService.CreateAsync(paymentOptions, null, ct);

        // Return the payment to requesting method
        return new StripePayment(
          createdPayment.CustomerId,
          createdPayment.ReceiptEmail,
          createdPayment.Description,
          createdPayment.Currency,
          createdPayment.Amount,
          createdPayment.Id);
    }
}