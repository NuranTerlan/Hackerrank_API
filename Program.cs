using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Text;
using System;

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;


namespace Hackerrank_API
{
    class BaseDataModel
    {
        public int page { get; set; }
        public int per_page { get; set; }
        public int total { get; set; }
        public int total_pages { get; set; }
    }

    class DataModel : BaseDataModel
    {
        public List<MatchModel> data { get; set; }
    }

    class WinnerDataModel : BaseDataModel
    {
        public List<WinnerModel> data { get; set; }
    }

    class WinnerModel
    {
        public string name { get; set; }
        public string country { get; set; }
        public int year { get; set; }
        public string winner { get; set; }
        public string runnerup { get; set; }
    }

    class MatchModel
    {
        public string competition { get; set; }
        public int year { get; set; }
        public string round { get; set; }
        public string team1 { get; set; }
        public string team2 { get; set; }
        public string team1goals { get; set; }
        public string team2goals { get; set; }
    }

    class Result
    {
        public static int getNumDraws(int year)
        {
            int res = 0;

            using (var client = new HttpClient())
            {
                int checkGoals = 6;
                string mainURL = $"https://jsonmock.hackerrank.com/api/football_matches?year={year}";
                string query;
                Console.Write("Data is processing . . . ");
                Console.WriteLine($"Here is the list for {year} : \n");
                for (int i = 0; i < checkGoals; i++)
                {
                    query = $"{mainURL}&team1goals={i}&team2goals={i}";
                    var result = getDataAsDataModel(client, query);
                    res += result.total;
                    Console.WriteLine($"-----> {i}-{i} => {result.total} drawn matches found");
                }
            }
            return res;
        }

        public static int getNumGoalsWinner(int year, string competition)
        {
            int goals = 0;

            string firstQuery =
                $"https://jsonmock.hackerrank.com/api/football_competitions?name={competition}&year={year}";
            using (var client = new HttpClient())
            {
                var res = getDataAsWinnerDataModel(client, firstQuery);
                var winnerData = res.data;
                string winnerTeam = winnerData[0].winner;
                Console.WriteLine($"--- Winner Team of {competition} in {year} is {winnerTeam.ToUpper()}\n");

                string secondQuery;
                for (int i = 1; i > 0; i++)
                {
                    secondQuery =
                        $"https://jsonmock.hackerrank.com/api/football_matches?year={year}&team1={winnerTeam}&competition={competition}&page={i}";
                    var resTeam1 = getDataAsDataModel(client, secondQuery);
                    if (resTeam1.data.Count == 0)
                    {
                        break;
                    }
                    else
                    {
                        foreach (var matchModel in resTeam1.data)
                        {
                            goals += int.Parse(matchModel.team1goals);
                        }
                    }
                }
                for (int i = 1; i > 0; i++)
                {
                    secondQuery =
                        $"https://jsonmock.hackerrank.com/api/football_matches?year={year}&team2={winnerTeam}&competition={competition}&page={i}";
                    var resTeam2 = getDataAsDataModel(client, secondQuery);
                    if (resTeam2.data.Count == 0)
                    {
                        break;
                    }
                    else
                    {
                        foreach (var matchModel in resTeam2.data)
                        {
                            goals += int.Parse(matchModel.team2goals);
                        }
                    }
                }
            }

            return goals;
        }

        private static DataModel getDataAsDataModel(HttpClient client, string query)
        {
            var resultData = new DataModel();
            var response = client.GetAsync(query).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content;
                resultData = responseContent.ReadAsAsync<DataModel>().GetAwaiter().GetResult();
                return resultData;
            }
            
            return resultData;
        }

        private static WinnerDataModel getDataAsWinnerDataModel(HttpClient client, string query)
        {
            var resultData = new WinnerDataModel();
            var response = client.GetAsync(query).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content;
                resultData = responseContent.ReadAsAsync<WinnerDataModel>().GetAwaiter().GetResult();
                return resultData;
            }

            return resultData;
        }
    }

    class Program
    {
        public static void Main(string[] args)
        {
            // ------------------------------------------------------------ FIRST PART

            Console.Write("Which year do you look for drawn matches? ");
            int year1 = Convert.ToInt32(Console.ReadLine().Trim());
            int result = Result.getNumDraws(year1);
            Console.WriteLine($"\nTotal number of drawn matches : {result}");

            // ------------------------------------------------------------ SECOND PART

            Console.Write("\n\nWhich year do you look for? ");
            int year2 = int.Parse(Console.ReadLine().Trim());
            Console.Write("Enter the competition name as a correct way : ");
            string competition = Console.ReadLine();
            int goalsResult = Result.getNumGoalsWinner(year2, competition);
            Console.WriteLine($"\nTotal number of goals by competition winner team during the league at current year => {goalsResult}");
        }
    }
}
