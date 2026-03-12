using System.ComponentModel.DataAnnotations;

public class BusinessPartnerCustomer
{
    [Required]
    public string CardCode { get; set; }   // viene de SP

    [Required]
    public string CardName { get; set; }

    public string? CardForeignName { get; set; }

    [Required]
    public string Currency { get; set; } = "LPS";

    [Required]
    public int GroupCode { get; set; }

    [Required]
    public string FederalTaxID { get; set; } = "000000000000";

    [Required]
    public string AdditionalID { get; set; }  // Identidad (txtCLNewIdentidad)

    public string? UnifiedFederalTaxID { get; set; }  // RTN (opcional en UI)

    [Required]
    public string Phone1 { get; set; }

    [Required]
    public string Cellular { get; set; }

    public string? Phone2 { get; set; }

    public string? EmailAddress { get; set; }

    public string? Notes { get; set; }

    // CAMPOS PERSONALIZADOS:
    [Required]
    public int SalesPersonCode { get; set; }

    public string Canal { get; set; }

    public int PayTermsGrpCode { get; set; }

    public int PriceListNum { get; set; } = 1;

    public bool Propiedad13 { get; set; } = true;

    public string Empresa { get; set; }


    // DIRECCIONES
    [Required]
    public BusinessPartnerAddress BillTo { get; set; }

    [Required]
    public BusinessPartnerAddress ShipTo { get; set; }
}

public class BusinessPartnerAddress
{
    [Required]
    public string AddressName { get; set; }

    [Required]
    public string Street { get; set; }

    [Required]
    public string StreetNo { get; set; }  // fecha nacimiento en UI

    [Required]
    public string GlobalLocationNumber { get; set; } // Sexo en UI

    [Required]
    public string Block { get; set; }   // Colonia

    [Required]
    public string City { get; set; }    // Municipio

    [Required]
    public string County { get; set; }  // Departamento

    [Required]
    public string State { get; set; }

    public string Country { get; set; } = "HN";

    [Required]
    public string AddressName3 { get; set; }  // Casa #

    public string? TaxCode { get; set; }
}
