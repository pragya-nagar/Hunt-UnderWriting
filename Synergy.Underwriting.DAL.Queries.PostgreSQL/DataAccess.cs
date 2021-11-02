using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Synergy.Common.DAL.Access.PostgreSQL;
using Synergy.Underwriting.DAL.Queries.Entities;

namespace Synergy.Underwriting.DAL.Queries.PostgreSQL
{
    public class DataAccess : BaseDataAccess
    {
        private const string Schema = "main";

        public DataAccess(ILoggerFactory loggerFactory, string nameOrConnectionString)
            : base(loggerFactory, nameOrConnectionString)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasDefaultSchema(Schema);

            builder.Entity<Property>().HasKey(x => x.Id);
            builder.Entity<Property>().Property(x => x.Id).ValueGeneratedNever();
            builder.Entity<Property>().Property(x => x.ParcelId);
            builder.Entity<Property>().Property(x => x.Address);
            builder.Entity<Property>().Property(x => x.City);
            builder.Entity<Property>().Property(x => x.ZipCode);
            builder.Entity<Property>().Property(x => x.LeadId);
            builder.Entity<Property>().HasOne(x => x.State).WithMany().HasForeignKey(x => x.StateId);
            builder.Entity<Property>().HasOne(x => x.Lead).WithMany().HasForeignKey(x => x.LeadId);
            builder.Entity<Property>().HasOne(x => x.InternalLandUseCode).WithMany().HasForeignKey(x => x.InternalLandUseCodeId);
            builder.Entity<Property>().HasOne(x => x.GeneralLandUseCode).WithMany().HasForeignKey(x => x.GeneralLandUseCodeId);
            builder.Entity<Property>().HasMany(x => x.Delinquencies);
            builder.Entity<Property>().HasMany(x => x.Attachments);

            builder.Entity<Delinquency>().HasKey(x => x.Id);
            builder.Entity<Delinquency>().Property(x => x.Id).ValueGeneratedNever();
            builder.Entity<Delinquency>().Property(x => x.Year).HasColumnName("DelinquencyTaxYear");
            builder.Entity<Delinquency>().Property(x => x.RUAmount);
            builder.Entity<Delinquency>().Property(x => x.LTVPercent);
            builder.Entity<Delinquency>().Property(x => x.RULTVPercent);
            builder.Entity<Delinquency>().HasOne(x => x.Property).WithMany(x => x.Delinquencies).HasForeignKey(x => x.PropertyId);
            builder.Entity<Delinquency>().HasOne(x => x.Event).WithMany(x => x.Delinquencies).HasForeignKey(x => x.EventId);
            builder.Entity<Delinquency>().HasOne(x => x.Result).WithOne(x => x.Delinquency).HasForeignKey<Result>(x => x.DelinquencyId);
            builder.Entity<Delinquency>().HasOne(x => x.SupplementalData).WithOne(x => x.Delinquency).HasForeignKey<SupplementalData>(x => x.DelinquencyId);
            builder.Entity<Delinquency>().HasMany(x => x.Decisions).WithOne(x => x.Delinquency).HasForeignKey(x => x.DelinquencyId);
            builder.Entity<Delinquency>().HasMany(x => x.PropertyProfileDelinquencies).WithOne(x => x.Delinquency).HasForeignKey(x => x.DelinquencyId);
            builder.Entity<Delinquency>().HasMany(x => x.DelinquencyPropertyDisplayStrategy).WithOne(x => x.Delinquency).HasForeignKey(x => x.DelinquencyId);

            builder.Entity<DelinquencyAudit>().HasKey(x => x.InsertedOn);
            builder.Entity<DelinquencyAudit>().Property(x => x.InsertedOn).ValueGeneratedOnAdd();
            builder.Entity<DelinquencyAudit>().Property(x => x.Year).HasColumnName("DelinquencyTaxYear");

            builder.Entity<DelinquencyComment>().HasKey(x => x.Id);
            builder.Entity<DelinquencyComment>().HasOne(x => x.Author).WithMany().HasForeignKey(x => x.AuthorId);

            builder.Entity<DecisionAudit>().HasKey(x => x.InsertedOn);
            builder.Entity<DecisionAudit>().Property(x => x.InsertedOn).ValueGeneratedOnAdd();

            builder.Entity<Event>().HasKey(x => x.Id);
            builder.Entity<Event>().Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Entity<Event>().Property(x => x.EventNumber);
            builder.Entity<Event>().HasMany(x => x.Bids);
            builder.Entity<Event>().HasMany(x => x.Delinquencies);
            builder.Entity<Event>().HasMany(x => x.DecisionLevels);
            builder.Entity<Event>().HasOne(x => x.State);

            builder.Entity<EventAudit>().HasKey(x => x.InsertedOn);
            builder.Entity<EventAudit>().Property(x => x.InsertedOn).ValueGeneratedOnAdd();

            builder.Entity<PropertyAttachment>().HasKey(x => x.Id);
            builder.Entity<PropertyAttachment>().Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Entity<PropertyAttachment>().Property(x => x.TypeId).HasColumnName("PropertyAttachmentTypeId");
            builder.Entity<PropertyAttachment>().HasOne(x => x.Type).WithMany().HasForeignKey(x => x.TypeId);
            builder.Entity<PropertyAttachment>().HasOne(x => x.Property).WithMany(x => x.Attachments).HasForeignKey(x => x.PropertyId);

            builder.Entity<Bid>().HasKey(x => x.Id);
            builder.Entity<Bid>().Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Entity<Bid>().Property(x => x.Number).IsRequired();
            builder.Entity<Bid>().Property(x => x.Entity);
            builder.Entity<Bid>().Property(x => x.Portfolio);
            builder.Entity<Bid>().HasOne(x => x.Event).WithMany(x => x.Bids).HasForeignKey(x => x.EventId);

            builder.Entity<PropertyAudit>().HasKey(x => x.InsertedOn);
            builder.Entity<PropertyAudit>().Property(x => x.InsertedOn).ValueGeneratedOnAdd();

            builder.Entity<Lead>().HasKey(x => x.Id);
            builder.Entity<Lead>().Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Entity<Lead>().Property(x => x.MailingAddress1);
            builder.Entity<Lead>().Property(x => x.MailingAddress2);
            builder.Entity<Lead>().Property(x => x.MailingAddress3);
            builder.Entity<Lead>().Property(x => x.MailingCity);
            builder.Entity<Lead>().Property(x => x.MailingStateId);
            builder.Entity<Lead>().Property(x => x.MailingZipCode);
            builder.Entity<Lead>().HasOne(x => x.State).WithMany(x => x.Lead).HasForeignKey(x => x.MailingStateId);

            builder.Entity<LeadAudit>().HasKey(x => x.InsertedOn);
            builder.Entity<LeadAudit>().Property(x => x.InsertedOn).ValueGeneratedOnAdd();
            builder.Entity<LeadAudit>().HasOne(x => x.State).WithMany(x => x.LeadAudit).HasForeignKey(x => x.MailingStateId);

            builder.Entity<PropertyValuation>().HasKey(x => x.Id);
            builder.Entity<PropertyValuation>().Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Entity<PropertyValuation>().Property(x => x.PropertyId);
            builder.Entity<PropertyValuation>().Property(x => x.LandValue);
            builder.Entity<PropertyValuation>().Property(x => x.AppraisedValue);
            builder.Entity<PropertyValuation>().Property(x => x.ImprovementValue);
            builder.Entity<PropertyValuation>().Property(x => x.AppraisedYear);

            builder.Entity<PropertyValuationAudit>().HasKey(x => x.InsertedOn);
            builder.Entity<PropertyValuationAudit>().Property(x => x.InsertedOn).ValueGeneratedOnAdd();

            builder.Entity<DelinquencyPropertyDisplayStrategy>().HasKey(x => x.Id);
            builder.Entity<DelinquencyPropertyDisplayStrategy>().Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Entity<DelinquencyPropertyDisplayStrategy>().HasOne(x => x.PropertyDisplayStrategy)
                .WithMany(x => x.DelinquencyPropertyDisplayStrategy)
                .HasForeignKey(x => x.PropertyDisplayStrategyId);

            builder.Entity<DelinquencyPropertyDisplayStrategyAudit>().HasKey(x => x.InsertedOn);
            builder.Entity<DelinquencyPropertyDisplayStrategyAudit>().Property(x => x.InsertedOn).ValueGeneratedOnAdd();

            builder.Entity<Result>().HasKey(x => x.Id);
            builder.Entity<Result>().Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Entity<Result>().Property(x => x.BidNumber).IsRequired();
            builder.Entity<Result>().Property(x => x.TaxAmount).IsRequired();
            builder.Entity<Result>().Property(x => x.InterestRate).IsRequired();

            builder.Entity<SupplementalData>().ToTable("PropertySupplementalEventData");
            builder.Entity<SupplementalData>().HasKey(x => x.Id);
            builder.Entity<SupplementalData>().Property(x => x.Id).ValueGeneratedOnAdd();

            builder.Entity<SupplementalDataAudit>().ToTable("PropertySupplementalEventDataAudit");
            builder.Entity<SupplementalDataAudit>().HasKey(x => x.InsertedOn);
            builder.Entity<SupplementalDataAudit>().Property(x => x.InsertedOn).ValueGeneratedOnAdd();

            builder.Entity<DelinquencyPropertyScoring>();
            builder.Entity<DelinquencyPropertyScoring>().HasKey(x => x.DelinquencyId);
            builder.Entity<DelinquencyPropertyScoring>().Property(x => x.DelinquencyId).ValueGeneratedOnAdd();

            builder.Entity<DelinquencyPropertyScoringAudit>();
            builder.Entity<DelinquencyPropertyScoringAudit>().HasKey(x => x.InsertedOn);
            builder.Entity<DelinquencyPropertyScoringAudit>().Property(x => x.InsertedOn).ValueGeneratedOnAdd();

            builder.Entity<State>().HasKey(x => x.Id);
            builder.Entity<State>().Property(x => x.Id).ValueGeneratedNever();
            builder.Entity<State>().Property(x => x.Name);
            builder.Entity<State>().Property(x => x.Abbreviation);

            builder.Entity<StateAudit>().HasKey(x => x.InsertedOn);
            builder.Entity<StateAudit>().Property(x => x.InsertedOn).ValueGeneratedOnAdd();

            builder.Entity<County>().HasKey(x => x.Id);
            builder.Entity<County>().Property(x => x.Id).ValueGeneratedNever();
            builder.Entity<County>().Property(x => x.Name);

            builder.Entity<CountyAudit>().HasKey(x => x.InsertedOn);
            builder.Entity<CountyAudit>().Property(x => x.InsertedOn).ValueGeneratedOnAdd();

            builder.Entity<EventType>().HasKey(x => x.Id);
            builder.Entity<EventType>().Property(x => x.Id).ValueGeneratedNever();
            builder.Entity<EventType>().Property(x => x.Name);

            builder.Entity<EventTypeAudit>().HasKey(x => x.InsertedOn);
            builder.Entity<EventTypeAudit>().Property(x => x.InsertedOn).ValueGeneratedOnAdd();

            builder.Entity<AuctionType>().HasKey(x => x.Id);
            builder.Entity<AuctionType>().Property(x => x.Id).ValueGeneratedNever();
            builder.Entity<AuctionType>().Property(x => x.Name);

            builder.Entity<AuctionTypeAudit>().HasKey(x => x.InsertedOn);
            builder.Entity<AuctionTypeAudit>().Property(x => x.InsertedOn).ValueGeneratedOnAdd();

            builder.Entity<GeneralLandUseCode>().HasKey(x => x.Id);
            builder.Entity<GeneralLandUseCode>().Property(x => x.Id).ValueGeneratedNever();
            builder.Entity<GeneralLandUseCode>().Property(x => x.Name);

            builder.Entity<InternalLandUseCode>().HasKey(x => x.Id);
            builder.Entity<InternalLandUseCode>().Property(x => x.Id).ValueGeneratedNever();
            builder.Entity<InternalLandUseCode>().Property(x => x.Name);
            builder.Entity<InternalLandUseCode>().Property(x => x.Description);

            builder.Entity<DataCutRule>().HasKey(x => x.Id);
            builder.Entity<DataCutRule>().Property(x => x.Id).ValueGeneratedNever();
            builder.Entity<DataCutRule>().Property(x => x.Name);
            builder.Entity<DataCutRule>().Property(x => x.CountyId);
            builder.Entity<DataCutRule>().Property(x => x.DataCutResultTypeId);
            builder.Entity<DataCutRule>().HasOne(x => x.DataCutResultType).WithMany().HasForeignKey(x => x.DataCutResultTypeId);
            builder.Entity<DataCutRule>().HasMany(x => x.DataCutRuleItems);

            builder.Entity<DataCutRuleItem>().HasKey(x => x.Id);
            builder.Entity<DataCutRuleItem>().Property(x => x.Id).ValueGeneratedNever();

            builder.Entity<DataCutResultType>().HasKey(x => x.Id);
            builder.Entity<DataCutResultType>().Property(x => x.Id).ValueGeneratedNever();
            builder.Entity<DataCutResultType>().Property(x => x.Name);
            builder.Entity<DataCutResultType>().Property(x => x.Description);

            builder.Entity<EventDataCutRule>();
            builder.Entity<EventDataCutRule>().HasKey(x => x.Id);
            builder.Entity<EventDataCutRule>().Property(x => x.EventDataCutStrategyId);
            builder.Entity<EventDataCutRule>().Property(x => x.Id).ValueGeneratedNever();
            builder.Entity<EventDataCutRule>().HasOne(x => x.DataCutRule).WithMany(x => x.EventRuleLinks).HasForeignKey(x => x.DataCutRuleId);
            builder.Entity<EventDataCutRule>().HasOne(x => x.EventDataCutStrategy).WithMany(x => x.EventDataCutRuleLinks).HasForeignKey(x => x.EventDataCutStrategyId);

            builder.Entity<EventDataCutStrategy>();
            builder.Entity<EventDataCutStrategy>().HasKey(x => x.Id);
            builder.Entity<EventDataCutStrategy>().Property(x => x.Id).ValueGeneratedNever();
            builder.Entity<EventDataCutStrategy>().Property(x => x.EventId);
            builder.Entity<EventDataCutStrategy>().Property(x => x.IsActive);

            builder.Entity<PropertyProfileLogicType>().HasKey(x => x.Id);
            builder.Entity<PropertyProfileLogicType>().Property(x => x.Id).ValueGeneratedNever();
            builder.Entity<PropertyProfileLogicType>().Property(x => x.Name);
            builder.Entity<PropertyProfileLogicType>().Property(x => x.Description);

            builder.Entity<PropertyProfileFieldType>().HasKey(x => x.Id);
            builder.Entity<PropertyProfileFieldType>().Property(x => x.Id).ValueGeneratedNever();
            builder.Entity<PropertyProfileFieldType>().Property(x => x.Name);
            builder.Entity<PropertyProfileFieldType>().Property(x => x.Description);
            builder.Entity<PropertyProfileFieldType>().HasMany(x => x.PropertyProfileLogicTypes);

            builder.Entity<PropertyProfileRuleField>().HasKey(x => x.Id);
            builder.Entity<PropertyProfileRuleField>().Property(x => x.Id).ValueGeneratedNever();
            builder.Entity<PropertyProfileRuleField>().Property(x => x.Name);
            builder.Entity<PropertyProfileRuleField>().Property(x => x.Description);
            builder.Entity<PropertyProfileRuleField>().Property(x => x.PropertyProfileFieldTypeId);
            builder.Entity<PropertyProfileRuleField>().HasOne(x => x.PropertyProfileFieldType);

            builder.Entity<PropertyProfileRule>().HasKey(x => x.Id);
            builder.Entity<PropertyProfileRule>().Property(x => x.Id).ValueGeneratedNever();
            builder.Entity<PropertyProfileRule>().Property(x => x.Name);
            builder.Entity<PropertyProfileRule>().HasMany(x => x.PropertyProfileRuleItems);

            builder.Entity<PropertyProfileRuleItem>().HasKey(x => x.Id);
            builder.Entity<PropertyProfileRuleItem>().Property(x => x.Id).ValueGeneratedNever();
            builder.Entity<PropertyProfileRuleItem>().HasOne(x => x.PropertyProfileRuleField);
            builder.Entity<PropertyProfileRuleItem>().HasOne(x => x.PropertyProfileLogicType);
            builder.Entity<PropertyProfileRuleItem>().HasMany(x => x.PropertyProfileRuleItemValues);

            builder.Entity<PropertyProfileRuleItemValue>().HasKey(x => x.Id);
            builder.Entity<PropertyProfileRuleItemValue>().Property(x => x.Id).ValueGeneratedNever();
            builder.Entity<PropertyProfileRuleItemValue>().Property(x => x.Value);

            builder.Entity<PropertyProfile>().HasKey(x => x.Id);
            builder.Entity<PropertyProfile>().Property(x => x.Id).ValueGeneratedNever();
            builder.Entity<PropertyProfile>().Property(x => x.Name);
            builder.Entity<PropertyProfile>().Property(x => x.IsActive);
            builder.Entity<PropertyProfile>().HasMany(x => x.PropertyProfileStates);
            builder.Entity<PropertyProfile>().HasMany(x => x.EventDecisionLevelPropertyProfile);

            builder.Entity<PropertyProfileState>().HasKey(x => x.Id);
            builder.Entity<PropertyProfileState>().Property(x => x.Id).ValueGeneratedNever();
            builder.Entity<PropertyProfileState>().Property(x => x.StateId);
            builder.Entity<PropertyProfileState>().HasOne(x => x.PropertyProfile).WithMany(x => x.PropertyProfileStates).HasForeignKey(x => x.PropertyProfileId);

            builder.Entity<EventDataCutDecision>().HasKey(x => x.Id);
            builder.Entity<EventDataCutDecision>().Property(x => x.Id).ValueGeneratedNever();
            builder.Entity<EventDataCutDecision>().HasOne(x => x.EventDataCutStrategy);
            builder.Entity<EventDataCutDecision>().HasOne(x => x.Delinquency);

            builder.Entity<EventDataCutDecisionAudit>().HasKey(x => x.InsertedOn);
            builder.Entity<EventDataCutDecisionAudit>().Property(x => x.InsertedOn).ValueGeneratedOnAdd();

            builder.Entity<PropertyProfileDelinquency>().HasKey(x => x.Id);
            builder.Entity<PropertyProfileDelinquency>().Property(x => x.Id).ValueGeneratedNever();
            builder.Entity<PropertyProfileDelinquency>().HasOne(x => x.Delinquency);
            builder.Entity<PropertyProfileDelinquency>().HasOne(x => x.PropertyProfile);

            builder.Entity<EventDecisionLevelPropertyProfile>().HasKey(x => x.Id);
            builder.Entity<EventDecisionLevelPropertyProfile>().Property(x => x.Id).ValueGeneratedNever();
            builder.Entity<EventDecisionLevelPropertyProfile>().HasOne(x => x.PropertyProfile);

            builder.Entity<EventDecisionLevel>().HasKey(x => x.Id);
            builder.Entity<EventDecisionLevel>().Property(x => x.Id).ValueGeneratedNever();
            builder.Entity<EventDecisionLevel>().HasMany(x => x.EventDecisionLevelUser);
            builder.Entity<EventDecisionLevel>().HasMany(x => x.Decisions);

            builder.Entity<EventDecisionLevelUser>().HasKey(x => x.Id);
            builder.Entity<EventDecisionLevelUser>().Property(x => x.Id).ValueGeneratedNever();
            builder.Entity<EventDecisionLevelUser>().HasOne(x => x.EventDecisionLevel);
            builder.Entity<EventDecisionLevelUser>().HasOne(x => x.EventDecisionLevelPropertyProfile);

            builder.Entity<DepartmentRole>().HasKey(x => x.Id);
            builder.Entity<DepartmentRole>().Property(x => x.Id).ValueGeneratedNever();
            builder.Entity<DepartmentRole>().HasOne(x => x.Department);

            builder.Entity<Department>().HasKey(x => x.Id);
            builder.Entity<Department>().Property(x => x.Id).ValueGeneratedNever();

            builder.Entity<UserRole>().HasKey(x => x.Id);
            builder.Entity<UserRole>().Property(x => x.Id).ValueGeneratedNever();
            builder.Entity<UserRole>().HasOne(x => x.User);

            builder.Entity<User>().HasKey(x => x.Id);
            builder.Entity<User>().Property(x => x.Id).ValueGeneratedNever();

            base.OnModelCreating(builder);
        }
    }
}
