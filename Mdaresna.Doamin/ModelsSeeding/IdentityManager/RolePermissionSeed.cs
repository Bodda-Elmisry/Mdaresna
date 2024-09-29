using Mdaresna.Doamin.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.ModelsSeeding.IdentityManager
{
    public class RolePermissionSeed : IEntityTypeConfiguration<RolePermission>
    {
        

        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            InitAppManagerRolePermissions(builder);
            InitStanderdPermissions(builder);
            InitSchoolManagerPermissions(builder);
        }


        private void InitAppManagerRolePermissions(EntityTypeBuilder<RolePermission> builder)
        {

            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("228AE7F5-C704-4660-AEB0-0E1F43112AE1"),
                PermissionId = Guid.Parse("76BEFB0E-5EF7-4FE3-8E06-02D50C17C38B")
            });

            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("228AE7F5-C704-4660-AEB0-0E1F43112AE1"),
                PermissionId = Guid.Parse("FDEBAADE-4D2C-447B-BA52-11E3E2A622F4")
            });

            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("228AE7F5-C704-4660-AEB0-0E1F43112AE1"),
                PermissionId = Guid.Parse("219007EA-620E-4D96-8292-2D015EF68DB1")
            });

            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("228AE7F5-C704-4660-AEB0-0E1F43112AE1"),
                PermissionId = Guid.Parse("8A48610D-C9FF-4FCE-95EB-3A4D8D633A9C")
            });

            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("228AE7F5-C704-4660-AEB0-0E1F43112AE1"),
                PermissionId = Guid.Parse("072C7A82-B62D-45D8-A3ED-48A70319ABF3")
            });

            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("228AE7F5-C704-4660-AEB0-0E1F43112AE1"),
                PermissionId = Guid.Parse("71B6F158-82F2-4A48-9950-56DBB00EEC4E")
            });

            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("228AE7F5-C704-4660-AEB0-0E1F43112AE1"),
                PermissionId = Guid.Parse("0225DBD5-9675-438C-87F2-63FB6921841C")
            });

            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("228AE7F5-C704-4660-AEB0-0E1F43112AE1"),
                PermissionId = Guid.Parse("69A0778B-A7A8-4E47-B8FD-D061428DBB95")
            });

            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("228AE7F5-C704-4660-AEB0-0E1F43112AE1"),
                PermissionId = Guid.Parse("68713DF9-ED1B-449C-8060-D919A2592B02")
            });

            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("228AE7F5-C704-4660-AEB0-0E1F43112AE1"),
                PermissionId = Guid.Parse("122D8E7A-F5A7-48EA-B2A6-F86964A2C3E7")
            });
        }

        private void InitStanderdPermissions(EntityTypeBuilder<RolePermission> builder)
        {
            builder.HasData(new RolePermission
            {
                PermissionId = Guid.Parse("219007EA-620E-4D96-8292-2D015EF68DB1"),
                RoleId = Guid.Parse("92D00B28-9D25-4BD2-A587-6C22A3A07A92")
            });
        }

        private void InitSchoolManagerPermissions(EntityTypeBuilder<RolePermission> builder)
        {
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("BE86C901-37F9-4F63-B7B3-03653E75FEA5")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("B15E32EB-092E-437D-9D4E-B9ED583C23B0")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("32D821BB-0C50-4721-9034-097019632C05")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("A4A93CEB-21F9-4400-9444-0D94CF895BEB")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("6ED86695-9EA7-4820-BF89-0DE16DFD6EA9")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("FDEBAADE-4D2C-447B-BA52-11E3E2A622F4")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("E50A5830-E741-4260-820C-19C2CAB1B419")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("CB37BBD0-40AD-4CA8-AE19-1ED87FEA0B97")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("062FF041-C775-4B30-8A49-202DB7D9DD28")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("D512C1B3-DDE0-4646-98CC-2BE5C56C7150")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("49869E47-2209-4FD8-AA08-2E99BA211FF1")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("E5B8454F-A193-4D63-B791-3A02AA9D71D3")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("0D9BFAD2-0762-47BA-8046-3AA92B2D6222")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("D9B2C724-C5CC-497A-A11B-3E48B7DBFD97")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("98D37ADE-FDC0-4A0B-8E00-402A708C9DDD")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("B7BCF498-F7E9-48B4-9E99-49F6D0A9A956")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("77127635-8FE8-4451-AC9C-59C38EE02D4D")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("2F1290B1-F439-4810-BA25-5C3E9CC56EFE")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("26DE4395-48FB-4684-B88D-5C9E6081F9AC")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("869A6521-2730-4F0B-973B-60FB8093C769")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("B6858FE8-DCC3-4C0F-B1A9-6A8AB7331924")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("DE53CF00-E53D-4BE0-8F67-71F8C7248DF4")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("861AD498-A214-4B7D-BDE7-815ABF63A587")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("494D9C56-558A-427C-9854-878520FCDEC8")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("FBA69A66-4E31-44F2-93AC-9602705A7F98")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("21040E37-D949-42EA-9A77-96AED0289209")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("11F0AA92-E7AF-4F94-BD50-96F389DEEE2A")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("3B49FC12-D345-41C4-B5D0-97A388AAAFA3")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("9DD22E15-9701-492B-AE20-985B8927F3BF")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("7A382B52-95F3-442F-ABFF-9C1C5C9607CC")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("93FD0AA6-852F-4AFE-8CF4-9CDEB365D498")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("7B4478C8-2526-493A-A34D-9FFFA5786F85")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("30AA5979-36F6-4A63-98D2-A06F96178176")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("AD53E402-C2AB-4941-861B-A19C82DCA0CC")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("B6D6C7E2-E2DE-4B95-9789-A61C69F27765")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("9301FC37-AE75-4EF6-B6FA-A656452E5A2E")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("ECD2236F-6FA1-43EC-92E9-AA36AE2358B4")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("EEC01FE0-4399-44B0-B8E7-AFD417A6A93C")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("99FDFBA4-EF5B-44D1-B9BF-B4FAF68A7B60")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("E09A3A9F-0FF1-46B7-8C80-B7F07CCB4542")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("D9BDCF84-D193-4775-8F9B-BACB1BD22915")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("EBD73C8F-2EF9-4A67-99E6-BAFE71C9E83C")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("A992E74B-4CE4-4D18-BCC5-C079D116D1F5")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("16983650-564E-4331-97D2-C1B5D67FEF40")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("056ED8BB-3FFC-459F-831E-C8A462DB313B")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("84956C26-E46F-432A-9B44-C8FB17E95552")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("B65324A3-35B9-4579-8C85-C9938391DFA6")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("CC4A8F98-FBB7-49DD-8B5F-CC6D02BCCAB0")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("C1856505-7C6B-4932-8034-CF4FBC0EFB67")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("C50D6574-648B-4F6E-8ED5-D27F8B5A34F1")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("65B57F13-161F-43B2-9E8D-DAAFB50FBBBB")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("FE286E0A-193B-4F12-AE88-DF5808FB2EB7")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("B7F4D3F3-B0B9-4E2F-8884-E2D61E0B6F5E")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("E831E41D-1090-4AD0-BBAF-E4CA823ECD9D")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("5C8D7610-BFAA-4DEB-BD84-E54ED4351611")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("BAA954FD-8BA9-4529-834D-E640057998AF")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("DD0F56AD-6039-4A8B-9D26-EBD5ABE87E7D")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("EE6325A4-4BC4-4AA7-9944-F82C3B7A305A")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("F19CCD22-35AC-4778-8A7F-FB0E56822385")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("09E73F16-BB7A-403B-A8F9-FDC1C3228B0D")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("A1134102-7021-4770-8808-FDC376190691")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("75162229-2536-4232-B081-FEABE20C318D")
            });
            builder.HasData(new RolePermission
            {
                RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                PermissionId = Guid.Parse("6DE52188-1C80-46E3-A436-FF36469E2976")
            });
        }


    }



}
