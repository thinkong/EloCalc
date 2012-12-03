using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSRCalc
{
    public class DotaPlayerPSR
    {
        public DotaPlayerPSR(string s, double winpoint, double losepoint)
        {
            Name = s;
            fWinPoint = winpoint;
            fLosePoint = losepoint;
        }
        public string Name { get; set; }
        public double fWinPoint { get; set; }
        public double fLosePoint { get; set; }
    }
    public class DotaPSR
    {
        static readonly double psf_baseKFactor = 20;
        //var psf_gammaCurveK = 18;
        //var psf_gammaCurveRange = 200;
        //var psf_gammaCurveTheta = 5.0000;
        static readonly double psf_KFactorScale = 8;
        static readonly double psf_logisticPredictionScale = 80.0000;
        static readonly double psf_maxKFactor = 40;
        static readonly double psf_medianScalingRank = 1600;
        static readonly double psf_minKFactor = 10;
        static readonly double psf_teamRankWeighting = 6.5000;

        public string sDebugString { get; set; }
        public double iTeam1WinPerc { get; set; }
        public double iTeam2WinPerc { get; set; }

        public Dictionary<string, DotaPlayerPSR> cWinLoseDic = new Dictionary<string, DotaPlayerPSR>();

        public DotaPSR(List<Player> team1, List<Player> team2)
        {
            sDebugString = "";
            double[] team1weights = new double[team1.Count];// = new List<int>();
            double[] team2weights = new double[team2.Count];// = new List<int>();
            double team1weight = 0;
            double team2weight = 0;
            double team1totalpsr = 0;
            double team2totalpsr = 0;
            for (int i = 0; i < team1.Count; i++)
            {
                team1weights[i] = Math.Pow(team1[i].PSR, psf_teamRankWeighting);
                team1weight += team1weights[i];
                team1totalpsr += team1[i].PSR;
            }
            for (int i = 0; i < team2.Count; i++)
            {
                team2weights[i] = Math.Pow(team2[i].PSR, psf_teamRankWeighting);
                team2weight += team2weights[i];
                team2totalpsr += team2[i].PSR;
            }
            double team1avgpsr = team1totalpsr / team1.Count;
            double team2avgpsr = team2totalpsr / team2.Count;
            sDebugString += ("team1numplayers=" + team1.Count + Environment.NewLine);
            sDebugString += ("team2numplayers=" + team2.Count + Environment.NewLine);
            sDebugString += (Environment.NewLine);
            sDebugString += ("team1avgpsr=" + team1avgpsr + Environment.NewLine);
            sDebugString += ("team2avgpsr=" + team2avgpsr + Environment.NewLine);
            sDebugString += (Environment.NewLine);
            double team1rating = Math.Pow(team1weight, 1.0 / psf_teamRankWeighting);
            double team2rating = Math.Pow(team2weight, 1.0 / psf_teamRankWeighting);
            double diff = team1rating - team2rating;
            double team1winprobability = 1.0 / (1.0 + Math.Pow(Math.E, ((-1.0 * diff) / psf_logisticPredictionScale)));
            double team2winprobability = 1.0 - team1winprobability;
            double[] team1kfactors = new double[team1.Count];// = [];
            double[] team2kfactors = new double[team2.Count];// = [];
            for (var i = 0; i < team1.Count; i++)
            {
                team1kfactors[i] = ((psf_medianScalingRank - team1[i].PSR) / psf_KFactorScale) + psf_baseKFactor;
                team1kfactors[i] = Math.Min(team1kfactors[i], psf_maxKFactor);
                team1kfactors[i] = Math.Max(team1kfactors[i], psf_minKFactor);
                double distancefromteamavgpsr = team1[i].PSR - 50 - team1avgpsr;
                if (distancefromteamavgpsr > 100)
                {
                    distancefromteamavgpsr = 100;
                }
                if (distancefromteamavgpsr > 0 && distancefromteamavgpsr <= 100)
                {
                    var implayingwithnewbiesfactor = (100.0 - distancefromteamavgpsr) / 100;
                    // sDebugString+= ("(scaling team1 player#" + (i + 1) + " kfactor by=" + implayingwithnewbiesfactor + ")");
                    team1kfactors[i] = implayingwithnewbiesfactor * team1kfactors[i];
                }
            }
            for (var i = 0; i < team2.Count; i++)
            {
                team2kfactors[i] = ((psf_medianScalingRank - team2[i].PSR) / psf_KFactorScale) + psf_baseKFactor;
                team2kfactors[i] = Math.Min(team2kfactors[i], psf_maxKFactor);
                team2kfactors[i] = Math.Max(team2kfactors[i], psf_minKFactor);
                double distancefromteamavgpsr = team2[i].PSR - 50 - team2avgpsr;
                if (distancefromteamavgpsr > 100)
                {
                    distancefromteamavgpsr = 100;
                }
                if (distancefromteamavgpsr > 0 && distancefromteamavgpsr <= 100)
                {
                    double implayingwithnewbiesfactor = (100.0 - distancefromteamavgpsr) / 100;
                    // sDebugString+= ("(scaling team2 player#" + (i + 1) + " kfactor by=" + implayingwithnewbiesfactor + ")");
                    team2kfactors[i] = implayingwithnewbiesfactor * team2kfactors[i];
                }
            }
            double[] team1winpts = new double[team1.Count];// = [];
            double[] team1losepts = new double[team1.Count];// = [];
            double[] team2winpts = new double[team2.Count];// = [];
            double[] team2losepts = new double[team2.Count];// = [];
            for (int i = 0; i < team1.Count; i++)
            {
                team1winpts[i] = Math.Ceiling(team2winprobability * team1kfactors[i]);
                team1losepts[i] = Math.Floor(team1winprobability * team1kfactors[i]);
            }
            for (int i = 0; i < team2.Count; i++)
            {
                team2winpts[i] = Math.Ceiling(team1winprobability * team2kfactors[i]);
                team2losepts[i] = Math.Floor(team2winprobability * team2kfactors[i]);
            }

            iTeam1WinPerc = (team1winprobability * 100);
            iTeam2WinPerc = 100 - iTeam1WinPerc;
            sDebugString += ("team1win%=" + iTeam1WinPerc + Environment.NewLine);
            sDebugString += ("team2win%=" + iTeam2WinPerc + Environment.NewLine);
            sDebugString += (Environment.NewLine);
            sDebugString += ("team1kfactors=");
            foreach (double d in team1kfactors)
            {
                sDebugString += (d + " ");
            }
            sDebugString += ("team2kfactors=");
            foreach (double d in team2kfactors)
            {
                sDebugString += (d + " ");
            }

            sDebugString += (Environment.NewLine);
            sDebugString += ("team1 win/lose=") + Environment.NewLine;
            for (int i = 0; i < team1.Count; i++)
            {
                sDebugString += ("+" + team1winpts[i] + "/-" + team1losepts[i] + Environment.NewLine);
                DotaPlayerPSR cPlayer = new DotaPlayerPSR(team1[i].Name, team1winpts[i], -team1losepts[i]);
                cWinLoseDic.Add(cPlayer.Name, cPlayer);
            }
            sDebugString += (Environment.NewLine);

            sDebugString += ("team2 win/lose=") + Environment.NewLine;
            for (int i = 0; i < team2.Count; i++)
            {
                sDebugString += ("+" + team2winpts[i] + "/-" + team2losepts[i] + Environment.NewLine);
                DotaPlayerPSR cPlayer = new DotaPlayerPSR(team2[i].Name, team2winpts[i], -team2losepts[i]);
                cWinLoseDic.Add(cPlayer.Name, cPlayer);
            }
            sDebugString += (Environment.NewLine);
            // return str;
        }
        private static int parseInt(Player c)
        {
            return (int)c.PSR;
        }
    }
}
