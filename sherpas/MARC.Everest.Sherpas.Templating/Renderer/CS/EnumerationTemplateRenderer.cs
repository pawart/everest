﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MARC.Everest.Sherpas.Template.Interface;
using MARC.Everest.Sherpas.Templating.Format;
using System.CodeDom;
using System.ComponentModel;
using MARC.Everest.Attributes;

namespace MARC.Everest.Sherpas.Templating.Renderer.CS
{
    /// <summary>
    /// Represents the enumeration template renderer
    /// </summary>
    public class EnumerationTemplateRenderer : IArtifactRenderer
    {
        #region IArtifactRenderer Members

        /// <summary>
        /// Artifact template type that this can render
        /// </summary>
        /// <example>
        /// <code lang="xml">
        /// </code></example>
        public Type ArtifactTemplateType
        {
            get { return typeof(EnumerationTemplateDefinition); }
        }

        /// <summary>
        /// Render the enumeration
        /// </summary>
        public System.CodeDom.CodeTypeMemberCollection Render(RenderContext context)
        {
            var tpl = context.Artifact as EnumerationTemplateDefinition;
            
            // emit the enum
            CodeTypeDeclaration retVal = new CodeTypeDeclaration(tpl.Name);
            retVal.IsEnum = true;
            retVal.Attributes = MemberAttributes.Public;

            // Add structure attribute
            retVal.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(StructureAttribute)), 
                new CodeAttributeArgument("Name", new CodePrimitiveExpression(tpl.Name)),
                new CodeAttributeArgument("CodeSystem", new CodePrimitiveExpression(tpl.Id.First())),
                new CodeAttributeArgument("StructureType", new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(MARC.Everest.Attributes.StructureAttribute.StructureAttributeType)), "ValueSet")) 
            ));
            foreach (var id in tpl.Id)
                retVal.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(TemplateAttribute)), new CodeAttributeArgument("TemplateId", new CodePrimitiveExpression(id))));

            // Documentation
            if (tpl.Documentation != null)
            {
                retVal.Comments.Add(new CodeCommentStatement(new CodeComment(String.Format("<summary>{0} value set</summary>", tpl.Name), true)));
                retVal.Comments.Add(new CodeCommentStatement(new CodeComment("<remarks>", true)));
                foreach (var doc in tpl.Documentation)
                    retVal.Comments.Add(new CodeCommentStatement(new CodeComment(doc.OuterXml, true)));
                retVal.Comments.Add(new CodeCommentStatement(new CodeComment("</remarks>", true)));
            }
            if (tpl.Example != null)
            {
                retVal.Comments.Add(new CodeCommentStatement(new CodeComment("<example><code lang=\"xml\"><![CDATA[", true)));
                foreach (var ex in tpl.Example)
                    retVal.Comments.Add(new CodeCommentStatement(new CodeComment(ex.OuterXml, true)));
                retVal.Comments.Add(new CodeCommentStatement(new CodeComment("]]></code></example>", true)));
            }

            // Emit the members
            foreach (var value in tpl.Literal)
            {
                CodeMemberField literal = new CodeMemberField(new CodeTypeReference(tpl.Name), value.Literal);
                if (value.DisplayName != null)
                    literal.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(DescriptionAttribute)), new CodeAttributeArgument(new CodePrimitiveExpression(value.DisplayName))));
                literal.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(EnumerationAttribute)),
                    new CodeAttributeArgument("Value", new CodePrimitiveExpression(value.Code)),
                    new CodeAttributeArgument("SupplierDomain", new CodePrimitiveExpression(value.CodeSystem))
                ));
                retVal.Members.Add(literal);
            }

            return new CodeTypeMemberCollection(new CodeTypeMember[] { retVal });

        }

        #endregion
    }
}
