// Kota.Domain/Validation/LocationValidator.cs
using FluentValidation;
using Kota.Domain.Entities;

namespace Kota.Domain.Validation
{
    public class SiteValidator : AbstractValidator<Site>
    {
        public SiteValidator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(32);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
        }
    }

    public class BuildingValidator : AbstractValidator<Building>
    {
        public BuildingValidator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(32);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
            RuleFor(x => x.SiteId).GreaterThan(0);
        }
    }

    public class RoomValidator : AbstractValidator<Room>
    {
        public RoomValidator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(32);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
            RuleFor(x => x.BuildingId).GreaterThan(0);
        }
    }

    public class AreaValidator : AbstractValidator<Area>
    {
        public AreaValidator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(32);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
            RuleFor(x => x.RoomId).GreaterThan(0);
        }
    }

    public class StorageUnitValidator : AbstractValidator<StorageUnit>
    {
        public StorageUnitValidator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(32);
            RuleFor(x => x.Kind).NotEmpty().MaximumLength(16);
            RuleFor(x => x.Type).NotEmpty().MaximumLength(32);
            RuleFor(x => x.RoomId).GreaterThan(0);
        }
    }

    public class BinValidator : AbstractValidator<Bin>
    {
        public BinValidator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(64);
            RuleFor(x => x.Kind).NotEmpty().MaximumLength(16);
            RuleFor(x => x.StorageUnitId).GreaterThan(0);
        }
    }

    public class ItemValidator : AbstractValidator<Item>
    {
        public ItemValidator()
        {
            RuleFor(x => x.Description).NotEmpty().MaximumLength(255);
            RuleFor(x => x.ManufacturerSku).MaximumLength(64).When(x => !string.IsNullOrEmpty(x.ManufacturerSku));
            RuleFor(x => x.BinId).GreaterThan(0);
            RuleFor(x => x.QtyOnHand).GreaterThanOrEqualTo(0);
            RuleFor(x => x.MinQty).GreaterThanOrEqualTo(0);
        }
    }

    public class UserValidator : AbstractValidator<User>
    {
        public UserValidator()
        {
            RuleFor(x => x.Username).NotEmpty().MaximumLength(64);
            RuleFor(x => x.PasswordHash).NotEmpty().MaximumLength(255);
            RuleFor(x => x.Role).NotEmpty().Must(role => role == "admin" || role == "user")
                .WithMessage("Role must be either 'admin' or 'user'");
        }
    }
}