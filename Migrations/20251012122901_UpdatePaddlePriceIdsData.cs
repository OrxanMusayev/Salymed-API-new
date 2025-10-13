using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePaddlePriceIdsData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Basic Plan ($45/month)
            migrationBuilder.Sql(
                "UPDATE SubscriptionPlans SET PaddlePriceId = 'pri_01k7akm9vxby21janvh9av3ajf' WHERE Id = 1;");

            // Professional Plan ($75/month)
            migrationBuilder.Sql(
                "UPDATE SubscriptionPlans SET PaddlePriceId = 'pri_01k7aknda27yemng3tdama2pgs' WHERE Id = 2;");

            // Premium Plan ($125/month)
            migrationBuilder.Sql(
                "UPDATE SubscriptionPlans SET PaddlePriceId = 'pri_01k7akq2ynww0yhrz6ypf22890' WHERE Id = 3;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback - Price ID'leri NULL yap
            migrationBuilder.Sql(
                "UPDATE SubscriptionPlans SET PaddlePriceId = NULL WHERE Id IN (1, 2, 3);");
        }
    }
}
