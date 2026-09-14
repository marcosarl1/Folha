
using FluentValidation;
using Folha.Api.DTOs;

namespace Folha.Api.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Nome é obrigatório").MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Email inválido");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).WithMessage("Senha deve ter no mínimo 6 caracteres");
    }

    public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
    {
        public CreateCategoryRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Nome da categoria é obrigatório").MaximumLength(50).WithMessage("Nome deve ter no máximo 50 caracteres");
        }

        public class CreateTransactionRequestValidator : AbstractValidator<CreateTransactionRequest>
        {
            public CreateTransactionRequestValidator()
            {
                RuleFor(x => x.Description).NotEmpty().MaximumLength(200).WithMessage("Descrição é obrigatória");
                RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Valor deve ser maior que zero");
                RuleFor(x => x.CategoryId).NotEmpty().WithMessage("Categoria é obrigatória");
                RuleFor(x => x.Date).NotEmpty();
            }
        }
    }
}
