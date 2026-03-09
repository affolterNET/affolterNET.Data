using System;
using affolterNET.Data.DtoHelper.Database;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace affolterNET.Data.DtoHelper.CodeGen
{
    public class PropertyGenerator
    {
        private readonly Column _col;

        public PropertyGenerator(Column col)
        {
            _col = col;
        }

        private AccessorDeclarationSyntax GetAccessor()
        {
            return SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        private AccessorDeclarationSyntax SetAccessor()
        {
            return SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        public PropertyDeclarationSyntax Generate()
        {
            if (string.IsNullOrWhiteSpace(_col.PropertyType))
            {
                throw new InvalidOperationException($"{nameof(_col.PropertyType)} was empty");
            }

            var type = SyntaxFactory.ParseTypeName(_col.PropertyType);
            if (string.IsNullOrWhiteSpace(_col.PropertyName))
            {
                throw new InvalidOperationException($"{nameof(_col.PropertyName)} was empty");
            }
            var name = _col.PropertyName;
            var propertyDeclaration = SyntaxFactory.PropertyDeclaration(type, name)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(GetAccessor(), SetAccessor());

            // primary key
            if (_col.IsPK)
            {
                propertyDeclaration = propertyDeclaration.WithAttributeLists(
                    SyntaxFactory.SingletonList(
                        SyntaxFactory.AttributeList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Key"))))));
            }

            return propertyDeclaration;
        }
    }
}