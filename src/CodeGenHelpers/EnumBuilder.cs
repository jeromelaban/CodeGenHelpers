﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace CodeGenHelpers
{
    public class EnumBuilder : IBuilder
    {
        private readonly List<string> _attributes = new List<string>();
        private readonly List<EnumValueBuilder> _values = new List<EnumValueBuilder>();

        internal EnumBuilder(string name, CodeBuilder builder)
        {
            Name = name;
            Builder = builder;
        }

        public CodeBuilder Builder { get; }

        public string Name { get; }

        public Accessibility? AccessModifier { get; private set; }

        public EnumValueBuilder AddValue(string name, int? numericValue = null)
        {
            var builder = new EnumValueBuilder(name, this, numericValue);
            _values.Add(builder);
            return builder;
        }

        public EnumBuilder AddNamespaceImport(string importedNamespace)
        {
            Builder.AddNamespaceImport(importedNamespace);
            return this;
        }

        public EnumBuilder AddNamespaceImport(ISymbol symbol)
        {
            Builder.AddNamespaceImport(symbol);
            return this;
        }

        public EnumBuilder AddNamespaceImport(INamespaceSymbol symbol)
        {
            Builder.AddNamespaceImport(symbol);
            return this;
        }

        public EnumBuilder AddAttribute(string attribute)
        {
            var sanitized = attribute.Replace("[", string.Empty).Replace("]", string.Empty);
            if (!_attributes.Contains(sanitized))
                _attributes.Add(sanitized);

            return this;
        }

        public EnumBuilder MakePublicEnum() => WithAccessModifier(Accessibility.Public);

        public EnumBuilder MakeInternalEnum() => WithAccessModifier(Accessibility.Internal);

        public EnumBuilder WithAccessModifier(Accessibility accessModifier)
        {
            AccessModifier = accessModifier;
            return this;
        }

        void IBuilder.Write(ref CodeWriter writer)
        {
            var queue = new Queue<IBuilder>();
            _values.OrderBy(x => x.Value)
                .ThenBy(x => x.Name)
                .ToList()
                .ForEach(x => queue.Enqueue(x));

            foreach(var attr in _attributes.OrderBy(x => x))
            {
                writer.AppendLine($"[{attr}]");
            }

            using (writer.Block($"{AccessModifier.Code()} enum {Name}"))
            {
                while(queue.Any())
                {
                    var value = queue.Dequeue();
                    value.Write(ref writer);

                    if(queue.Any())
                    {
                        writer.AppendUnindentedLine(",");
                        writer.NewLine();
                    }
                    else
                    {
                        writer.NewLine();
                    }
                }
            }
        }
    }
}
