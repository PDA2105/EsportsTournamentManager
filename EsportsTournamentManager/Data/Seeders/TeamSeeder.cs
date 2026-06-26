using System;
using System.Collections.Generic;
using System.Linq;
using EsportsTournamentManager.Models;

namespace EsportsTournamentManager.Data.Seeders
{
    public static class TeamSeeder
    {
        public static void Seed(AppDbContext context)
        {
            // Seed Team 1
            if (!context.Teams.Any(t => t.Acronym == "T1"))
            {
                var t1 = new Team
                {
                    TeamName = "T1 Esports",
                    Acronym = "T1",
                    Coach = "Kim 'kkOma' Jeong-gyun",
                    Region = "LCK",
                    CreatedDate = DateTime.Now,
                    Players = new List<Player>
                    {
                        new Player { InGameName = "Faker", RealName = "Lee Sang-hyeok", Position = "Mid", IsActive = true },
                        new Player { InGameName = "Zeus", RealName = "Choi Woo-je", Position = "Top", IsActive = true },
                        new Player { InGameName = "Oner", RealName = "Mun Hyeon-jun", Position = "Jungle", IsActive = true },
                        new Player { InGameName = "Gumayusi", RealName = "Lee Min-hyeong", Position = "Bot (ADC)", IsActive = true },
                        new Player { InGameName = "Keria", RealName = "Ryu Min-seok", Position = "Support", IsActive = true },
                        new Player { InGameName = "Rekkles", RealName = "Carl Martin Erik Larsson", Position = "Sub (Dự bị)", IsActive = true }
                    }
                };
                context.Teams.Add(t1);
            }

            // Seed Team 2
            if (!context.Teams.Any(t => t.Acronym == "GEN"))
            {
                var gen = new Team
                {
                    TeamName = "Gen.G Esports",
                    Acronym = "GEN",
                    Coach = "Kim 'KIM' Jeong-soo",
                    Region = "LCK",
                    CreatedDate = DateTime.Now,
                    Players = new List<Player>
                    {
                        new Player { InGameName = "Chovy", RealName = "Jeong Ji-hoon", Position = "Mid", IsActive = true },
                        new Player { InGameName = "Kiin", RealName = "Kim Gi-in", Position = "Top", IsActive = true },
                        new Player { InGameName = "Canyon", RealName = "Kim Geon-bu", Position = "Jungle", IsActive = true },
                        new Player { InGameName = "Peyz", RealName = "Kim Su-hwan", Position = "Bot (ADC)", IsActive = true },
                        new Player { InGameName = "Lehends", RealName = "Son Si-woo", Position = "Support", IsActive = true },
                        new Player { InGameName = "Slayer", RealName = "Kim Jin-young", Position = "Sub (Dự bị)", IsActive = false }
                    }
                };
                context.Teams.Add(gen);
            }

            // Seed Team 3
            if (!context.Teams.Any(t => t.Acronym == "HLE"))
            {
                var hle = new Team
                {
                    TeamName = "Hanwha Life Esports",
                    Acronym = "HLE",
                    Coach = "Choi 'DanDy' In-kyu",
                    Region = "LCK",
                    CreatedDate = DateTime.Now,
                    Players = new List<Player>
                    {
                        new Player { InGameName = "Zeka", RealName = "Kim Geon-woo", Position = "Mid", IsActive = true },
                        new Player { InGameName = "Doran", RealName = "Choi Hyeon-joon", Position = "Top", IsActive = true },
                        new Player { InGameName = "Peanut", RealName = "Han Wang-ho", Position = "Jungle", IsActive = true },
                        new Player { InGameName = "Viper", RealName = "Park Do-hyeon", Position = "Bot (ADC)", IsActive = true },
                        new Player { InGameName = "Delight", RealName = "Yoo Hwan-joong", Position = "Support", IsActive = true },
                        new Player { InGameName = "Grizzly", RealName = "Cho Seung-hoon", Position = "Sub (Dự bị)", IsActive = false }
                    }
                };
                context.Teams.Add(hle);
            }

            // Seed Team 4
            if (!context.Teams.Any(t => t.Acronym == "DK"))
            {
                var dk = new Team
                {
                    TeamName = "Dplus KIA",
                    Acronym = "DK",
                    Coach = "Lee 'Zefa' Jae-min",
                    Region = "LCK",
                    CreatedDate = DateTime.Now,
                    Players = new List<Player>
                    {
                        new Player { InGameName = "ShowMaker", RealName = "Heo Su", Position = "Mid", IsActive = true },
                        new Player { InGameName = "Kingen", RealName = "Hwang Seong-hoon", Position = "Top", IsActive = true },
                        new Player { InGameName = "Lucid", RealName = "Choi Yong-hyeok", Position = "Jungle", IsActive = true },
                        new Player { InGameName = "Aiming", RealName = "Kim Ha-ram", Position = "Bot (ADC)", IsActive = true },
                        new Player { InGameName = "Moham", RealName = "Lee Jeong-hun", Position = "Support", IsActive = true },
                        new Player { InGameName = "Kellin", RealName = "Kim Hyeong-gyu", Position = "Sub (Dự bị)", IsActive = false }
                    }
                };
                context.Teams.Add(dk);
            }

            context.SaveChanges();
        }
    }
}
