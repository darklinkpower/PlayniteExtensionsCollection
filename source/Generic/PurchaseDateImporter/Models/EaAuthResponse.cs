using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurchaseDateImporter.Models
{
    public partial class EaAuthResponse
    {
        [SerializationPropertyName("access_token")]
        public string AccessToken { get; set; }

        [SerializationPropertyName("token_type")]
        public string TokenType { get; set; }

        [SerializationPropertyName("expires_in")]
        public string ExpiresIn { get; set; }
    }

    public partial class EaIdentityResponse
    {
        [SerializationPropertyName("pid")]
        public Pid Pid { get; set; }
    }

    public partial class Pid
    {
        [SerializationPropertyName("externalRefType")]
        public string ExternalRefType { get; set; }

        [SerializationPropertyName("externalRefValue")]
        public string ExternalRefValue { get; set; }

        [SerializationPropertyName("pidId")]
        public long PidId { get; set; }

        [SerializationPropertyName("email")]
        public string Email { get; set; }

        [SerializationPropertyName("emailStatus")]
        public string EmailStatus { get; set; }

        [SerializationPropertyName("strength")]
        public string Strength { get; set; }

        [SerializationPropertyName("dob")]
        public DateTimeOffset Dob { get; set; }

        [SerializationPropertyName("country")]
        public string Country { get; set; }

        [SerializationPropertyName("language")]
        public string Language { get; set; }

        [SerializationPropertyName("locale")]
        public string Locale { get; set; }

        [SerializationPropertyName("status")]
        public string Status { get; set; }

        [SerializationPropertyName("reasonCode")]
        public string ReasonCode { get; set; }

        [SerializationPropertyName("tosVersion")]
        public string TosVersion { get; set; }

        [SerializationPropertyName("parentalEmail")]
        public string ParentalEmail { get; set; }

        [SerializationPropertyName("thirdPartyOptin")]
        public string ThirdPartyOptin { get; set; }

        [SerializationPropertyName("globalOptin")]
        public string GlobalOptin { get; set; }

        [SerializationPropertyName("dateCreated")]
        public string DateCreated { get; set; }

        [SerializationPropertyName("dateModified")]
        public string DateModified { get; set; }

        [SerializationPropertyName("lastAuthDate")]
        public string LastAuthDate { get; set; }

        [SerializationPropertyName("registrationSource")]
        public string RegistrationSource { get; set; }

        [SerializationPropertyName("authenticationSource")]
        public string AuthenticationSource { get; set; }

        [SerializationPropertyName("showEmail")]
        public string ShowEmail { get; set; }

        [SerializationPropertyName("discoverableEmail")]
        public string DiscoverableEmail { get; set; }

        [SerializationPropertyName("anonymousPid")]
        public string AnonymousPid { get; set; }

        [SerializationPropertyName("underagePid")]
        public string UnderagePid { get; set; }

        [SerializationPropertyName("defaultBillingAddressUri")]
        public string DefaultBillingAddressUri { get; set; }

        [SerializationPropertyName("defaultShippingAddressUri")]
        public string DefaultShippingAddressUri { get; set; }

        [SerializationPropertyName("passwordSignature")]
        public long PasswordSignature { get; set; }
    }

    public partial class EaEntitlementsResponse
    {
        [SerializationPropertyName("entitlements")]
        public List<Entitlement> Entitlements { get; set; }
    }

    public partial class Entitlement
    {
        [SerializationPropertyName("entitlementId")]
        public long EntitlementId { get; set; }

        [SerializationPropertyName("offerId")]
        public string OfferId { get; set; }

        [SerializationPropertyName("entitlementTag")]
        public string EntitlementTag { get; set; }

        [SerializationPropertyName("grantDate")]
        public DateTime GrantDate { get; set; }

        [SerializationPropertyName("status")]
        public string Status { get; set; }

        [SerializationPropertyName("useCount")]
        public long UseCount { get; set; }

        [SerializationPropertyName("entitlementType")]
        public string EntitlementType { get; set; }

        [SerializationPropertyName("originPermissions")]
        public string OriginPermissions { get; set; }

        [SerializationPropertyName("updatedDate")]
        public DateTimeOffset UpdatedDate { get; set; }

        [SerializationPropertyName("productCatalog")]
        public string ProductCatalog { get; set; }

        [SerializationPropertyName("isConsumable")]
        public bool IsConsumable { get; set; }

        [SerializationPropertyName("version")]
        public long Version { get; set; }

        [SerializationPropertyName("suppressedOfferIds")]
        public string[] SuppressedOfferIds { get; set; }

        [SerializationPropertyName("suppressedBy")]
        public string[] SuppressedBy { get; set; }

        [SerializationPropertyName("offerType")]
        public string OfferType { get; set; }

        [SerializationPropertyName("originDisplayType")]
        public string OriginDisplayType { get; set; }

        [SerializationPropertyName("offerAccess")]
        public string OfferAccess { get; set; }

        [SerializationPropertyName("masterTitleId")]
        public string MasterTitleId { get; set; }

        [SerializationPropertyName("gameEditionTypeFacetKeyRankDesc")]
        public string GameEditionTypeFacetKeyRankDesc { get; set; }
    }
}