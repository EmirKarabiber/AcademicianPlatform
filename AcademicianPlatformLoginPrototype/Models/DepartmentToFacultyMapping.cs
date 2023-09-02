

using Microsoft.CodeAnalysis.Options;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;

public static class DepartmentToFacultyMapping
{
    public static Dictionary<string, string> MapDepartmentsToFaculties()
    {
        var mapping = new Dictionary<string, string>
        {
            { "Bilgisayar Mühendisliği", "Mühendislik ve Doğa Bilimleri" },
            { "Yazılım Mühendisliği", "Mühendislik ve Doğa Bilimleri" },
            { "Endüstri Mühendisliği", "Mühendislik ve Doğa Bilimleri" },
            { "Endüstriyel Tasarım Mühendisliği", "Mühendislik ve Doğa Bilimleri" },
            { "Kimya Mühendisliği", "Mühendislik ve Doğa Bilimleri" },
            { "Makine Mühendisliği", "Mühendislik ve Doğa Bilimleri" },
            { "Mekatronik Mühendisliği", "Mühendislik ve Doğa Bilimleri" },
            { "Moleküler Biyoloji ve Genetik", "Mühendislik ve Doğa Bilimleri" },
            { "Mimarlık", "Mühendislik ve Doğa Bilimleri" },
            { "Tıp", "Tıp" },
            { "Diş Hekimliği", "Diş Hekimliği" },
            { "Eczacılık", "Eczacılık" },
            { "Beslenme ve Diyetetik", "Sağlık Bilimleri" },
            { "Ergoterapi", "Sağlık Bilimleri" },
            { "Fizyoterapi ve Rehabilitasyon", "Sağlık Bilimleri" },
            { "Odyoloji", "Sağlık Bilimleri" },
            { "Hemşirelik", "Sağlık Bilimleri" },
            //{ "Hemşirelik", "Sağlık Bilimleri" },
            { "İngiliz Dili ve Edebiyatı", "İktisadi, İdari ve Sosyal Bilimler" },
            { "İşletme", "İktisadi, İdari ve Sosyal Bilimler" },
            { "Psikoloji", "İktisadi, İdari ve Sosyal Bilimler" },
            { "Anestezi", "Sağlık Meslek Yüksekokulu" },
            { "Ağız ve Diş Sağlığı", "Sağlık Meslek Yüksekokulu" },
            { "İlk ve Acil Yardım", "Sağlık Meslek Yüksekokulu" },
            { "Tıbbi Görüntüleme Teknikleri", "Sağlık Meslek Yüksekokulu" },
            { "Tıbbi Laboratuvar Teknikleri", "Sağlık Meslek Yüksekokulu" },
            { "İngilizce Mütercim-Tercümanlık", "Yabancı Diller Yüksekokulu" },
            { "İngilizce Hazırlık Birimi", "Yabancı Diller Yüksekokulu" },
            { "Ortak Yabancı Dil Dersleri Birimi", "Yabancı Diller Yüksekokulu" },
            // Diğer departmanlar ve fakülteler burada eşlenir
        };

        return mapping;
    }
}