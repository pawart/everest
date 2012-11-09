/* 
 * Copyright 2008-2012 Mohawk College of Applied Arts and Technology
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: Justin Fyfe
 * Date: 01-09-2009
 */
using System;
using System.Collections.Generic;
using System.Text;
using MARC.Everest.DataTypes.Interfaces;
using MARC.Everest.Connectors;
using MARC.Everest.DataTypes;
using MARC.Everest.Exceptions;
using MARC.Everest.Xml;
using System.Reflection;

namespace MARC.Everest.Formatters.XML.Datatypes.R1.Formatters
{
    /// <summary>
    /// Data type R1 formatter for ED
    /// </summary> 
    public class EDFormatter : IDatatypeFormatter
    {
        /// <summary>
        /// Host context
        /// </summary>
        public IXmlStructureFormatter Host { get; set; }

        /// <summary>
        /// Get or set the generic arguments to this type (if applicable)
        /// </summary>
        public Type[] GenericArguments { get; set; }

        #region IDatatypeFormatter Members

      
        /// <summary>
        /// Graph the object <paramref name="o"/> onto stream <paramref name="s"/>
        /// </summary>
        /// <param name="s">The XmlWriter to write the object to</param>
        /// <param name="o">The object to graph</param>
        public void Graph(System.Xml.XmlWriter s, object o, DatatypeFormatterGraphResult result)
        {
            // Get an instance ref
            ED instance_ed = (ED)o;

            // Do a base format
            ANYFormatter baseFormatter = new ANYFormatter();
            baseFormatter.Graph(s, o, result);
            
            // Null flavor
            if (((ANY)o).NullFlavor != null)
            {
                return;
            }

            // Attributes
            s.WriteAttributeString("representation", Util.ToWireFormat(instance_ed.Representation));
            if (instance_ed.MediaType != null)
                s.WriteAttributeString("mediaType", Util.ToWireFormat(instance_ed.MediaType));
            if (instance_ed.Language != null)
                s.WriteAttributeString("language", instance_ed.Language);
            if (instance_ed.Compression != null)
                s.WriteAttributeString("compression", Util.ToWireFormat(instance_ed.Compression));
            if (instance_ed.IntegrityCheck != null)
                s.WriteAttributeString("integrityCheck", Convert.ToBase64String(instance_ed.IntegrityCheck));
            if (instance_ed.Description != null)
                result.AddResultDetail(new UnsupportedDatatypeR1PropertyResultDetail(ResultDetailType.Warning, "Description", "ED", s.ToString()));
            if (instance_ed.IntegrityCheckAlgorithm != null)
            {
                // Incorrect representation of the SHA1 and SHA256 names in r1
                switch ((EncapsulatedDataIntegrityAlgorithm)instance_ed.IntegrityCheckAlgorithm)
                {
                    case EncapsulatedDataIntegrityAlgorithm.SHA1:
                        s.WriteAttributeString("integrityCheckAlgorithm", "SHA-1");
                        break;
                    case EncapsulatedDataIntegrityAlgorithm.SHA256:
                        s.WriteAttributeString("integrityCheckAlgorithm", "SHA-256");
                        break;
                }
            }

            // Elements
            if (instance_ed.Reference != null)
            {
                TELFormatter refFormatter = new TELFormatter();
                s.WriteStartElement("reference", "urn:hl7-org:v3");
                refFormatter.Graph(s, instance_ed.Reference, result);
                s.WriteEndElement();
            }
            if (instance_ed.Thumbnail != null)
            {
                EDFormatter thumbFormatter = new EDFormatter();
                s.WriteStartElement("thumbnail", "urn:hl7-org:v3");
                thumbFormatter.Graph(s, instance_ed.Thumbnail, result);
                s.WriteEndElement();
            }
            if (instance_ed.Translation != null)
                result.AddResultDetail(new UnsupportedDatatypeR1PropertyResultDetail(ResultDetailType.Warning, "Translation", "ED", s.ToString()));
            Encoding textEncoding = System.Text.Encoding.UTF8;

            // Value
            if (instance_ed.Data != null && instance_ed.Data.Length > 0)
            {
                if (instance_ed.Representation == EncapsulatedDataRepresentation.B64)
                    s.WriteBase64(instance_ed.Data, 0, instance_ed.Data.Length);
                else if (instance_ed.Representation == EncapsulatedDataRepresentation.TXT)
                    s.WriteString(textEncoding.GetString(instance_ed.Data, 0, instance_ed.Data.Length));
                else
                {
                    char[] charBuffer = textEncoding.GetChars(instance_ed.Data);
                    s.WriteRaw(charBuffer, 0, charBuffer.Length);
                }
            }
        }

        /// <summary>
        /// Parse the object
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object)")]
        public object Parse(System.Xml.XmlReader s, DatatypeFormatterParseResult result)
        {
            // Parse base (ANY) from the stream
            ANYFormatter baseFormatter = new ANYFormatter();
            string pathName = s is XmlStateReader ? (s as XmlStateReader).CurrentPath : s.Name;


            // Parse ED
            ED retVal = baseFormatter.Parse<ED>(s, result);

            // Now parse our data out... Attributes
            if (s.GetAttribute("representation") != null)
                retVal.Representation = (EncapsulatedDataRepresentation)Util.FromWireFormat(s.GetAttribute("representation"), typeof(EncapsulatedDataRepresentation));
            if (s.GetAttribute("mediaType") != null)
                retVal.MediaType = s.GetAttribute("mediaType");
            if (s.GetAttribute("language") != null)
                retVal.Language = s.GetAttribute("language");
            if (s.GetAttribute("compression") != null)
                retVal.Compression = (EncapsulatedDataCompression?)Util.FromWireFormat(s.GetAttribute("compression"), typeof(EncapsulatedDataCompression));
            if (s.GetAttribute("integrityCheckAlgorithm") != null)
            {
                switch(s.GetAttribute("integrityCheckAlgorithm"))
                {
                    case "SHA-1":
                        retVal.IntegrityCheckAlgorithm = EncapsulatedDataIntegrityAlgorithm.SHA1;
                        break;
                    case "SHA-256":
                        retVal.IntegrityCheckAlgorithm = EncapsulatedDataIntegrityAlgorithm.SHA256;
                        break;
                }
            }
            if (s.GetAttribute("integrityCheck") != null)
                retVal.IntegrityCheck = Convert.FromBase64String(s.GetAttribute("integrityCheck"));

            // Elements and inner data
            #region Elements
            string innerData = "";
            if (!s.IsEmptyElement)
            {
                // Exit markers
                int sDepth = s.Depth;
                string sName = s.Name;

                s.Read();
                // Read until exit condition is fulfilled
                while (!(s.NodeType == System.Xml.XmlNodeType.EndElement && s.Depth == sDepth && s.Name == sName))
                {
                    string oldName = s.Name; // Name
                    try
                    {
                        if (s.LocalName == "thumbnail") // Format using ED
                        {
                            EDFormatter edFormatter = new EDFormatter();
                            edFormatter.Host = this.Host;
                            retVal.Thumbnail = (ED)edFormatter.Parse(s, result); // Parse ED
                        }
                        else if (s.LocalName == "reference") // Format using TEL
                        {
                            TELFormatter telFormatter = new TELFormatter();
                            telFormatter.Host = this.Host;
                            retVal.Reference = (TEL)telFormatter.Parse(s, result);
                        }
                        else if (s.NodeType == System.Xml.XmlNodeType.Text ||
                            s.NodeType == System.Xml.XmlNodeType.CDATA)
                            innerData += s.Value;
                        else if (!(s.NodeType == System.Xml.XmlNodeType.EndElement && s.Depth == sDepth && s.Name == sName) &&
                            (s.NodeType == System.Xml.XmlNodeType.Element || s.NodeType == System.Xml.XmlNodeType.EndElement))
                        {
                            retVal.Representation = EncapsulatedDataRepresentation.XML;
                            innerData += s.ReadOuterXml();
                        }
                    }
                    catch (MessageValidationException e)
                    {
                        result.AddResultDetail(new MARC.Everest.Connectors.ResultDetail(MARC.Everest.Connectors.ResultDetailType.Error, e.Message, s.ToString(), e));
                    }
                    finally
                    {
                        if (s.Name == oldName) s.Read();
                    }
                }
            }
            #endregion

            Encoding textEncoding = System.Text.Encoding.UTF8;
            // Parse the innerData string into something meaningful
            if(innerData.Length > 0)
                if (retVal.Representation == EncapsulatedDataRepresentation.B64)
                    retVal.Data = Convert.FromBase64String(innerData);
                else
                    retVal.Data = textEncoding.GetBytes(innerData);

            // Finally, the hash, this will validate the data
            if(!retVal.ValidateIntegrityCheck())
                result.AddResultDetail(new ResultDetail(ResultDetailType.Warning,
                    string.Format("Encapsulated data with content starting with '{0}' failed integrity check!", retVal.ToString().PadRight(10, ' ').Substring(0, 10)), 
                    s.ToString(),
                    null));

            // Validate
            baseFormatter.Validate(retVal, pathName, result);

            return retVal;
        }

        /// <summary>
        /// Get the type that this formatter handles
        /// </summary>
        public string HandlesType
        {
            get { return "ED"; }
        }

        /// <summary>
        /// Get the supported properties for the rendering
        /// </summary>
        public List<PropertyInfo> GetSupportedProperties()
        {
            List<PropertyInfo> retVal = new List<PropertyInfo>(10);
            retVal.Add(typeof(ED).GetProperty("Representation"));
            retVal.Add(typeof(ED).GetProperty("MediaType"));
            retVal.Add(typeof(ED).GetProperty("Compression"));
            retVal.Add(typeof(ED).GetProperty("Language"));
            retVal.Add(typeof(ED).GetProperty("IntegrityCheck"));
            retVal.Add(typeof(ED).GetProperty("IntegrityCheckAlgorithm"));
            retVal.Add(typeof(ED).GetProperty("Reference"));
            retVal.Add(typeof(ED).GetProperty("Thumbnail"));
            retVal.Add(typeof(ED).GetProperty("Data"));
            retVal.AddRange(new ANYFormatter().GetSupportedProperties());
            return retVal;
        }
        #endregion
    }
}
