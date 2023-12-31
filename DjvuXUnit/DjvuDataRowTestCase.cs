﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using DiagnosticMessage = Xunit.Sdk.DiagnosticMessage;
using NullMessageSink = Xunit.Sdk.NullMessageSink;
using TestMethodDisplay = Xunit.Sdk.TestMethodDisplay;
using TestMethodDisplayOptions = Xunit.Sdk.TestMethodDisplayOptions;

#pragma warning disable CS8600, CS8602, CS8603
namespace DjvuNet.Tests.Xunit
{
    public class DjvuDataRowTestCase : TestMethodTestCase, IXunitTestCase
    {
        readonly IMessageSink _MessageSink;
        int _AttributeNumber;
        int _RowNumber;
        //int _Timeout;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use for deserialization only.")]
        public DjvuDataRowTestCase()
        {
            _MessageSink = new NullMessageSink();
        }

        public DjvuDataRowTestCase(
            IMessageSink messageSink,
            TestMethodDisplay methodDisplay,
            TestMethodDisplayOptions methodDisplayOptions,
            ITestMethod testMethod,
            int attributeNumber,
            int rowNumber)
            : base(
                  methodDisplay,
                  methodDisplayOptions,
                  testMethod,
                  GetTestMethodArguments(testMethod, attributeNumber, rowNumber, messageSink))
        {
            _AttributeNumber = attributeNumber;
            _RowNumber = rowNumber;
            _MessageSink = messageSink;
        }

        /// <summary>
        /// TODO implement interface memeber
        /// </summary>
        public virtual int Timeout { get; }

        protected virtual string GetDisplayName(IAttributeInfo factAttribute, string displayName)
        {
            return TestMethod.Method.GetDisplayNameWithArguments(displayName, TestMethodArguments, MethodGenericTypes);
        }

        protected virtual string GetSkipReason(IAttributeInfo factAttribute)
        {
            return factAttribute.GetNamedArgument<string>("Skip");
        }

        /// <inheritdoc/>
        protected override void Initialize()
        {
            base.Initialize();

            var factAttribute = TestMethod.Method.GetCustomAttributes(typeof(FactAttribute)).First();
            var baseDisplayName = factAttribute.GetNamedArgument<string>("DisplayName") ?? BaseDisplayName;

            DisplayName = GetDisplayName(factAttribute, baseDisplayName);
            SkipReason = GetSkipReason(factAttribute);

            foreach (var traitAttribute in GetTraitAttributesData(TestMethod))
            {
                var discovererAttribute = traitAttribute
                    .GetCustomAttributes(typeof(TraitDiscovererAttribute)).FirstOrDefault();

                if (discovererAttribute != null)
                {
                    var discoverer = ExtensibilityPointFactory.GetTraitDiscoverer(_MessageSink, discovererAttribute);
                    if (discoverer != null)
                        foreach (var keyValuePair in discoverer.GetTraits(traitAttribute))
                            Add(Traits, keyValuePair.Key, keyValuePair.Value);
                }
                else
                    _MessageSink.OnMessage(
                        new DiagnosticMessage(
                            $"Trait attribute on '{DisplayName}' did not have [TraitDiscoverer]"));
            }
        }

        static void Add<TKey, TValue>(IDictionary<TKey, List<TValue>> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
                dictionary[key].Add(value);
            else
                dictionary.Add(key, new List<TValue>(new TValue[] { value }));
        }

        static IEnumerable<IAttributeInfo> GetTraitAttributesData(ITestMethod testMethod)
        {
            return testMethod.TestClass.Class.Assembly.GetCustomAttributes(typeof(ITraitAttribute))
                .Concat(testMethod.Method.GetCustomAttributes(typeof(ITraitAttribute)))
                .Concat(testMethod.TestClass.Class.GetCustomAttributes(typeof(ITraitAttribute)));
        }

        static object[] GetTestMethodArguments(ITestMethod testMethod, int attributeNumber, int rowNumber, IMessageSink diagnosticMessageSink)
        {
            try
            {
                IAttributeInfo dataAttribute =
                    testMethod.Method.GetCustomAttributes(typeof(DataAttribute))
                    .Where((x, i) => i == attributeNumber)
                    .FirstOrDefault();

                if (dataAttribute == null)
                    return null;

                IAttributeInfo discovererAttribute =
                    dataAttribute.GetCustomAttributes(typeof(DataDiscovererAttribute)).First();

                IDataDiscoverer discoverer =
                    ExtensibilityPointFactory.GetDataDiscoverer(diagnosticMessageSink, discovererAttribute);

                return
                    discoverer.GetData(dataAttribute, testMethod.Method)
                    .Where((x, i) => i == rowNumber)
                    .FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public override void Serialize(IXunitSerializationInfo data)
        {
            data.AddValue("TestMethod", TestMethod);
            data.AddValue("AttributeNumber", _AttributeNumber);
            data.AddValue("RowNumber", _RowNumber);
        }

        public override void Deserialize(IXunitSerializationInfo data)
        {
            TestMethod = data.GetValue<ITestMethod>("TestMethod");
            _AttributeNumber = data.GetValue<int>("AttributeNumber");
            _RowNumber = data.GetValue<int>("RowNumber");
            TestMethodArguments = GetTestMethodArguments(TestMethod, _AttributeNumber, _RowNumber, _MessageSink);
        }

        protected override string GetUniqueID()
        {
            return
                $"{TestMethod.TestClass.Class.Name}:" +
                $"{TestMethod.Method.Name};{_AttributeNumber}/{_RowNumber}:" +
                $"{TestMethod.TestClass.TestCollection.TestAssembly.Assembly.Name};";
        }

        /// <inheritdoc/>
        public virtual Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink,
                                                 IMessageBus messageBus,
                                                 object[] constructorArguments,
                                                 ExceptionAggregator aggregator,
                                                 CancellationTokenSource cancellationTokenSource)
        {
            return new XunitTestCaseRunner(
                this,
                DisplayName,
                SkipReason,
                constructorArguments,
                TestMethodArguments,
                messageBus,
                aggregator,
                cancellationTokenSource)
                .RunAsync();
        }
    }
}
#pragma warning restore CS8600, CS8602, CS8603
