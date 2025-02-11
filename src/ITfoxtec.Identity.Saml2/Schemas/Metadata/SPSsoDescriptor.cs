﻿using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace ITfoxtec.Identity.Saml2.Schemas.Metadata
{
    /// <summary>
    /// The SPSSODescriptor element extends SSODescriptorType with content reflecting profiles specific
    /// to service providers. 
    /// </summary>
    public class SPSsoDescriptor : SsoDescriptorType
    {
        const string elementName = Saml2MetadataConstants.Message.SPSsoDescriptor;

        /// <summary>
        /// [Optional]
        /// Optional attribute that indicates whether the samlp:AuthnRequest messages sent by this
        /// service provider will be signed. If omitted, the value is assumed to be false.
        /// </summary>
        public bool? AuthnRequestsSigned { get; set; }

        /// <summary>
        /// [Optional]
        /// Optional attribute that indicates a requirement for the saml:Assertion elements received by
        /// this service provider to be signed. If omitted, the value is assumed to be false. This requirement
        /// is in addition to any requirement for signing derived from the use of a particular profile/binding
        /// combination.
        /// </summary>
        public bool? WantAssertionsSigned { get; set; }

        /// <summary>
        /// [Required]
        /// One element that describe indexed endpoints that support the profiles of the
        /// Authentication Request protocol defined in [SAMLProf]. All service providers support at least one
        /// such endpoint, by definition.
        /// </summary>
        public IEnumerable<AssertionConsumerService> AssertionConsumerServices { get; set; }

        /// <summary>
        /// [Optional]
        /// Zero or one element that describe an application or service provided by the service provider
        /// that requires or desires the use of SAML attributes.
        /// </summary>
        public IEnumerable<AttributeConsumingService> AttributeConsumingServices { get; set; }

        public XElement ToXElement()
        {
            var envelope = new XElement(Saml2MetadataConstants.MetadataNamespaceX + elementName);

            envelope.Add(GetXContent());

            return envelope;
        }

        protected IEnumerable<XObject> GetXContent()
        {
            yield return new XAttribute(Saml2MetadataConstants.Message.ProtocolSupportEnumeration, protocolSupportEnumeration);

            if(AuthnRequestsSigned.HasValue)
            {
                yield return new XAttribute(Saml2MetadataConstants.Message.AuthnRequestsSigned, AuthnRequestsSigned.Value);
            }

            if (WantAssertionsSigned.HasValue)
            {
                yield return new XAttribute(Saml2MetadataConstants.Message.WantAssertionsSigned, WantAssertionsSigned.Value);
            }

            if (EncryptionCertificates != null)
            {
                foreach(var encryptionCertificate in EncryptionCertificates)
                {
                    yield return KeyDescriptor(encryptionCertificate, Saml2MetadataConstants.KeyTypes.Encryption, EncryptionMethods);
                }                
            }

            if (SigningCertificates != null)
            {
                foreach (var signingCertificate in SigningCertificates)
                {
                    yield return KeyDescriptor(signingCertificate, Saml2MetadataConstants.KeyTypes.Signing);
                }
            }

            if (SingleLogoutServices != null)
            {
                foreach (var singleLogoutService in SingleLogoutServices)
                {
                    yield return singleLogoutService.ToXElement();
                }
            }

            if (ArtifactResolutionServices != null)
            {
                foreach (var artifactResolutionService in ArtifactResolutionServices)
                {
                    yield return artifactResolutionService.ToXElement();
                }
            }

            if (NameIDFormats != null)
            {
                foreach (var nameIDFormat in NameIDFormats)
                {
                    yield return new XElement(Saml2MetadataConstants.MetadataNamespaceX + Saml2MetadataConstants.Message.NameIDFormat, nameIDFormat.OriginalString);
                }
            }

            if (AssertionConsumerServices == null)
            {
                throw new ArgumentNullException("AssertionConsumerService property");
            }
            var index = 0;
            foreach (var assertionConsumerService in AssertionConsumerServices)
            {
                yield return assertionConsumerService.ToXElement(index++);
            }

            if (AttributeConsumingServices != null)
            {
                foreach (var attributeConsumingService in AttributeConsumingServices)
                {
                    yield return attributeConsumingService.ToXElement();
                }                
            }
        }


        protected internal SPSsoDescriptor Read(XmlElement xmlElement)
        {
            AuthnRequestsSigned = xmlElement.Attributes[Saml2MetadataConstants.Message.AuthnRequestsSigned]?.Value.Equals(true.ToString(), StringComparison.InvariantCultureIgnoreCase);

            WantAssertionsSigned = xmlElement.Attributes[Saml2MetadataConstants.Message.WantAssertionsSigned]?.Value.Equals(true.ToString(), StringComparison.InvariantCultureIgnoreCase);

            ReadKeyDescriptors(xmlElement);

            var assertionConsumerServicesElements = xmlElement.SelectNodes($"*[local-name()='{Saml2MetadataConstants.Message.AssertionConsumerService}']");
            if (assertionConsumerServicesElements != null)
            {
                AssertionConsumerServices = ReadAcsService(assertionConsumerServicesElements);
            }

            ReadArtifactResolutionService(xmlElement);

            ReadSingleLogoutService(xmlElement); 

            ReadNameIDFormat(xmlElement);

            return this;
        }

        protected IEnumerable<AssertionConsumerService> ReadAcsService(XmlNodeList acsElements) 
        {
            foreach (XmlNode singleLogoutServiceElement in acsElements)
            {
                yield return new AssertionConsumerService
                {
                    Binding = singleLogoutServiceElement.Attributes[Saml2MetadataConstants.Message.Binding].GetValueOrNull<Uri>(),
                    Location = singleLogoutServiceElement.Attributes[Saml2MetadataConstants.Message.Location].GetValueOrNull<Uri>(),
                    IsDefault = singleLogoutServiceElement.Attributes[Saml2MetadataConstants.Message.IsDefault].GetValueOrNull<bool>(),
                    Index = singleLogoutServiceElement.Attributes[Saml2MetadataConstants.Message.Index].GetValueOrNull<int>(),
                };
            }
        }
    }
}
