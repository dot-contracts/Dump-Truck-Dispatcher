using System;
using System.ComponentModel.DataAnnotations;
using Abp.Extensions;

namespace DispatcherWeb.Items.Dto
{
    public class GetLocationsWithRatesInput
    {
        public string Term { get; set; }

        public UnitOfMeasureBaseEnum UomBaseId { get; set; }

        public DesignationEnum Designation { get; set; }

        public int? MaterialItemId { get; set; }

        [Required]
        public int? FreightUomId { get; set; }

        public int? MaterialUomId { get; set; }

        [Required]
        public int? DeliverToId { get; set; }

        public int? LoadAtId { get; set; }

        [Required]
        public int? PricingTierId { get; set; }

        public void Validate()
        {
            switch (UomBaseId)
            {
                case UnitOfMeasureBaseEnum.DriveMiles:
                case UnitOfMeasureBaseEnum.AirMiles:
                case UnitOfMeasureBaseEnum.DriveKMs:
                case UnitOfMeasureBaseEnum.AirKMs:
                    break;
                default:
                    throw new ArgumentException("Invalid UomBaseId");
            }

            switch (Designation)
            {
                case DesignationEnum.FreightOnly:
                    break;

                case DesignationEnum.FreightAndMaterial:
                    if (MaterialUomId == null)
                    {
                        throw new ArgumentException("MaterialUomId is required");
                    }
                    break;

                default:
                    throw new ArgumentException("Invalid Designation");
            }

            if (MaterialItemId == null && LoadAtId == null)
            {
                throw new ArgumentException("MaterialItemId is required");
            }
        }

        public bool AreRequiredFieldsFilled()
        {
            return UomBaseId != 0
                && Designation.IsIn(DesignationEnum.FreightAndMaterial, DesignationEnum.FreightOnly)
                && (Designation != DesignationEnum.FreightAndMaterial || MaterialUomId.HasValue)
                && (MaterialItemId.HasValue || LoadAtId.HasValue)
                && FreightUomId.HasValue
                && DeliverToId.HasValue
                && LoadAtId.HasValue
                && PricingTierId.HasValue;
        }

        public static GetLocationsWithRatesInput CreateFromPricingInput(GetItemPricingInput input)
        {
            return new GetLocationsWithRatesInput
            {
                Term = null,
                Designation = input.Designation,
                MaterialItemId = input.MaterialItemId,
                FreightUomId = input.FreightUomId,
                MaterialUomId = input.MaterialUomId,
                DeliverToId = input.DeliverToId,
                LoadAtId = input.LoadAtId,
                PricingTierId = input.PricingTierId,
            };
        }
    }
}
