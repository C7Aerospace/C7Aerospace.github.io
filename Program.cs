using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;

/* I developed this program for some reason.
 * I changed my environment from Windows to Ubuntu so I can't use made tools.
 * Even if I can, I won't use made tools because it's an excellent chance to challenge myself.
 * And developing is *so much fun*.
 * Isoheptane
 * 2020/04/01 15:09 */

namespace BilibiliPrize
{
    public static class BilibiliAPI
    {
        public static string memberInformation1 = "https://api.bilibili.com/x/space/upstat?mid={0}";
        public static string memberInformation2 = "https://api.bilibili.com/x/relation/stat?vmid={0}";
        public static string memberInformation3 = "https://api.bilibili.com/x/space/acc/info?mid={0}";
        // There are three different APIs for different usages
        // API1 : Article / Video views and Likes
        // API2 : Following Status
        // API3 : Personal Information
        public static string videoInformation = "https://api.bilibili.com/x/web-interface/view?aid={0}";
        public static string videoInformationBV = "https://api.bilibili.com/x/web-interface/view?bvid={0}";
        public static string videoComments = "https://api.bilibili.com/x/v2/reply?jsonp=jsonp&;pn={1}&type=1&oid={0}";
        public static string relations = "https://api.bilibili.com/x/space/acc/relation?mid={0}";
    }
    public class User
    {
        public int uid;
        public string name;
        public int level;
        public List<String> comments;
        public bool followed = false; // It means you followed
        public bool follower = false; // It means he is your follower;
        public User(int uid,string name,int level, string comment)
        {
            this.uid = uid;
            this.name = name;
            this.level = level;
            comments = new List<string>();
            AddComments(comment);
        }
        public void AddComments(string comment)
        {
            comments.Add(comment);
        }
        public override string ToString() // Serialize Comments
        {
            string ret = string.Format("{0} [UID:{1},Level:{2}", name, uid, level);
            if (follower)
                ret += ",Follower";
            if (followed)
                ret += ",Followed";
            ret += "]";
            for(int i = 0;i<comments.Count;i++)
            {
                ret += string.Format("\n{0}:", (i + 1).ToString().PadLeft(4));
                string[] commentLines = comments[i].Replace("\\n", "\n").Split('\n');
                ret += "\"" + commentLines[0];
                for(int j = 1;j<commentLines.Length;j++)
                    ret += "\n      " + commentLines[j];
            }
            ret += "\"\n";
            return ret;
        }
    }
    class MainClass
    {
        /// <summary>
        /// Get response from website via HTTP
        /// </summary>
        /// <returns>What the website response.</returns>
        /// <param name="url">Website URL</param>
        public static string HttpGet(string url)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "text/plain;charset=UTF-8";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
            string responseString = reader.ReadToEnd();
            reader.Close();
            responseStream.Close();
            return responseString;
        }
        public static string HttpGet(string url,string cookie)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "text/plain;charset=UTF-8";
            request.CookieContainer = new CookieContainer();
            request.CookieContainer.Add(new Cookie("SESSDATA",cookie,"/","bilibili.com"));
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
            string responseString = reader.ReadToEnd();
            reader.Close();
            responseStream.Close();
            return responseString;
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("Bilibili Draw v0.1.0 by Isoheptane\n");

            string vid;
            string uid;
            VideoInfo videoInfo;
            MemberInfo1 memberInfo1;
            MemberInfo2 memberInfo2;
            MemberInfo3 memberInfo3;
            while (true)
            {
                Console.Write("Bilibili Member UID: ");
                uid = Console.ReadLine();
                // Get video information
                // MemberAPI 1
                string memberInfo1Json = HttpGet(string.Format(BilibiliAPI.memberInformation1, uid));
                memberInfo1 = JsonConvert.DeserializeObject<MemberInfo1>(memberInfo1Json);
                // MemberAPI 2
                string memberInfo2Json = HttpGet(string.Format(BilibiliAPI.memberInformation2, uid));
                memberInfo2 = JsonConvert.DeserializeObject<MemberInfo2>(memberInfo2Json);
                // MemberAPI 3
                string memberInfo3Json = HttpGet(string.Format(BilibiliAPI.memberInformation3, uid));
                memberInfo3 = JsonConvert.DeserializeObject<MemberInfo3>(memberInfo3Json);
                if (memberInfo1.code != 0)
                    Console.WriteLine("Member information is invalid , code {0} : {1}", memberInfo1.code, memberInfo1.message); // Check member info available
                else
                    break;
                if (memberInfo2.code != 0)
                    Console.WriteLine("Member information is invalid , code {0} : {1}", memberInfo2.code, memberInfo2.message); // Check member info available
                else
                    break;
                if (memberInfo3.code != 0)
                    Console.WriteLine("Member information is invalid , code {0} : {1}", memberInfo3.code, memberInfo3.message); // Check member info available
                else
                    break;
            }
            // Display User Information
            Console.WriteLine("\nMember Information:");
            Console.WriteLine("  Name: {0}", memberInfo3.data.name);
            Console.WriteLine("  Gender: {0}", memberInfo3.data.sex);
            Console.WriteLine("  Level: {0}", memberInfo3.data.level);
            Console.WriteLine("  Introduction: {0}", memberInfo3.data.sign);
            Console.WriteLine("  Video Views: {0}  Article Views: {1}  Likes: {2}", memberInfo1.data.archive.view.ToString().PadLeft(9)
                                                                                  , memberInfo1.data.article.view.ToString().PadLeft(9)
                                                                                  , memberInfo1.data.likes);
            Console.WriteLine("  Followed:    {0}  Followers:     {1}",memberInfo2.data.following.ToString().PadLeft(9)
                                                                      ,memberInfo2.data.follower.ToString().PadLeft(9));
            // Collect Cookies
            string SESSDATA;
            if(File.Exists("SESSDATA"))
                SESSDATA = File.ReadAllText("SESSDATA");
            else
            {
                Console.WriteLine("\nYou can delete \"cookie\" file in the program directory to restore SESSDATA.");
                Console.Write("Bilibili SESSDATA: ");
                SESSDATA = Console.ReadLine();
                File.WriteAllText("SESSDATA", SESSDATA);
            }
            // Check Video Information
            while (true)
            {
                Console.Write("\nBilibili video ID: ");
                vid = Console.ReadLine();
                // Get video information
                string videoInfoJson;
                if(vid.Substring(0,2)=="BV") // Check is BVID or not
                    videoInfoJson = HttpGet(string.Format(BilibiliAPI.videoInformationBV, vid));
                else
                    videoInfoJson = HttpGet(string.Format(BilibiliAPI.videoInformation, vid));
                videoInfo = JsonConvert.DeserializeObject<VideoInfo>(videoInfoJson);
                if (videoInfo.code != 0)
                    Console.WriteLine("Video information is invalid , code {0} : {1}", videoInfo.code, videoInfo.message); // Check video available
                else
                    break;
            }
            // Display Video Information
            Console.WriteLine("\nVideo information:");
            Console.WriteLine("  Title: {0}", videoInfo.data.title);
            Console.WriteLine("  AID/BVID: {0} / {1}", videoInfo.data.aid, videoInfo.data.bvid);
            Console.WriteLine("  Author: {0}", videoInfo.data.owner.name);
            Console.WriteLine("  Author UID: {0}", videoInfo.data.owner.mid);
            Console.WriteLine("  Description: ");
            foreach (string line in videoInfo.data.desc.Replace("\\n", "\n").Split('\n'))
                Console.WriteLine("    {0}", line);
            Console.WriteLine("  View: {0}  Danmaku: {1}  Comments: {2}", videoInfo.data.stat.view.ToString().PadLeft(8)
                                                                        , videoInfo.data.stat.danmaku.ToString().PadLeft(5)
                                                                        , videoInfo.data.stat.reply.ToString().PadLeft(7));
            Console.WriteLine("  Like: {0}  Coin: {1}  Favorite: {2}", videoInfo.data.stat.like.ToString().PadLeft(8)
                                                                        , videoInfo.data.stat.coin.ToString().PadLeft(8)
                                                                        , videoInfo.data.stat.favorite.ToString().PadLeft(7));
            Console.WriteLine("  Disike: {0}  Share: {1}", videoInfo.data.stat.dislike.ToString().PadLeft(6)
                                                         , videoInfo.data.stat.share.ToString().PadLeft(7));
            Console.Write("\nPress Enter to check comments.");
            Console.ReadLine();

            // Check Comments
            Console.WriteLine("Getting comment informations...");
            List<User> users = new List<User>();
            int page = 1;
            while(true)
            {
                string commentsJson = HttpGet(string.Format(BilibiliAPI.videoComments, videoInfo.data.aid, page));
                CommentsInfo commentsInfo = JsonConvert.DeserializeObject<CommentsInfo>(commentsJson);
                CommentsData data = commentsInfo.data;
                if (data.replies == null)
                    break;
                // Analyze Comments
                foreach (CommentsReply replyObject in data.replies)
                {
                    bool exist = false;
                    foreach (User user in users)
                        if (user.uid == replyObject.mid) // Check is user recorded
                        {
                            exist = true;
                            user.AddComments(replyObject.content.message);
                            break;
                        }
                    if (!exist)
                        users.Add(new User(replyObject.mid, replyObject.member.uname, replyObject.member.level_info.current_level, replyObject.content.message));
                    if (replyObject.replies != null) // Second layer searching, same as top layer
                    {
                        foreach (CommentsReply replyReplyObject in replyObject.replies)
                        {
                            exist = false;
                            foreach (User user in users)
                                if (user.uid == replyReplyObject.mid)
                                {
                                    exist = true;
                                    user.AddComments(replyReplyObject.content.message);
                                    break;
                                }
                            if (!exist)
                                users.Add(new User(replyReplyObject.mid, replyReplyObject.member.uname, replyReplyObject.member.level_info.current_level, replyReplyObject.content.message));
                        }
                    }
                }
                page++;
            }
            page--;
            Console.Write("\n{0} users replied and followed, press Enter to display.", users.Count);
            Console.ReadLine();
            // Display all users and comments
            foreach(User user in users)
            {
                Console.WriteLine("  " + user.ToString());
            }
            // Exceptions
            while (true)
            {
                Console.Write("Would you like to except yourself from prize list? <y/n> ");
                string input = Console.ReadLine();
                if (input == "y")
                {
                    foreach (User user in users)
                        if (user.uid == int.Parse(uid))
                        {
                            users.Remove(user);
                            break;
                        }
                    break;
                }
                else if (input == "n")
                {
                    break;
                }
            }
            /* This is the solution available without cookies
            Random random = new Random();
            Console.WriteLine("\n{0}user is in the list. Press Enter to randomly select one user.",users.Count);
            while(true)
            {
                Console.ReadLine();
                Console.WriteLine("  " + users[random.Next(0, users.Count)].ToString());
            } */
            Console.Write("\n{0} user is in the list.How many users would you like to give prize : ", users.Count);
            bool fansOnly;
            int prizeCount = int.Parse(Console.ReadLine());
            // Check is fans only
            while (true)
            {
                Console.Write("Followers only? <y/n> ");
                string input = Console.ReadLine();
                if (input == "y")
                {
                    fansOnly = true;
                    break;
                }
                else if (input == "n")
                {
                    fansOnly = false;
                    break;
                }
            }
            // Randomly select users.
            Random random = new Random();
            List<User> winners = new List<User>();
            int winnerCount = 0;
            Console.WriteLine();
            while(winnerCount!=prizeCount)
            {
                int target = random.Next(0,users.Count); // Random won't generate a number equals to its maxValue
                if (winners.Contains(users[target]))
                    continue;
                else
                {
                    Relation relation;
                    for (int i = 1; i < 3; i++) // Limit the max attempt count
                    {
                        string relationJson = HttpGet(String.Format(BilibiliAPI.relations, users[target].uid), SESSDATA);
                        relation = JsonConvert.DeserializeObject<Relation>(relationJson);
                        if(relation.code!=0)
                        {
                            Console.WriteLine("  Attempt to get relation about UID{0} information failed, code {1} : {2}", users[target].uid, relation.code, relation.message);
                            continue;
                        }
                        else
                        {
                            if (relation.data.be_relation.mid == int.Parse(uid)) // Check is fans or not
                                users[target].follower = true;
                            if (relation.data.relation.mid == users[target].uid)
                                users[target].followed = true;
                            break;
                        }
                    }
                    if (fansOnly)
                    {
                        if (users[target].follower)
                        {
                            winners.Add(users[target]);
                            winnerCount++;
                        }
                        continue;
                    }
                    winners.Add(users[target]);
                    winnerCount++;
                }
            }
            Console.WriteLine("Complete. Winners are:");
            foreach(User user in winners)
            {
                Console.WriteLine("  " + user.ToString());
            }
            Console.WriteLine("Congratulations!");
        }

        // It is really painful feelings.
        // Making Json Structure
        // Video Information API response Json Object
        public class VideoInfo
        {
            public int code { get; set; }
            public string message { get; set; }
            public VideoInfoData data { get; set; }
        }
        public class VideoInfoData
        {
            public string bvid { get; set; }
            public int aid { get; set; }
            public string title { get; set; }
            public string desc { get; set; }
            public VideoInfoDataOwner owner { get; set; }
            public VideoInfoDataStat stat { get; set; }
        }
        public class VideoInfoDataOwner
        {
            public int mid { get; set; }
            public string name { get; set; }
        }
        public class VideoInfoDataStat
        {
            public int view { get; set; }
            public int danmaku { get; set; }
            public int reply { get; set; }
            public int like { get; set; }
            public int dislike { get; set; }
            public int coin { get; set; }
            public int favorite { get; set; }
            public int share { get; set; }
        }
        public class CommentsInfo
        {
            public CommentsData data { get; set; }
        }
        public class CommentsData
        {
            public List<CommentsReply> replies { get; set; }
        }
        public class CommentsReply
        {
            public int mid { get; set; }
            public CommentsReplyMember member { get; set; }
            public CommentsReplyContent content { get; set; }
            public List<CommentsReply> replies { get; set; }
        }
        public class CommentsReplyMember
        {
            public string uname { get; set; }
            public CommentsReplyMemberLevelinfo level_info { get; set; }
        }
        public class CommentsReplyMemberLevelinfo
        {
            public int current_level { get; set; }
        }
        public class CommentsReplyContent
        {
            public string message { get; set; }
        }
        public class MemberInfo1
        {
            public int code { get; set; }
            public string message { get; set; }
            public MemberInfo1Data data { get; set; }
        }
        public class MemberInfo1Data
        {
            public MemberInfo1DataVideo archive { get; set; }
            public MemberInfo1DataArticle article { get; set; }
            public int likes { get; set; }
        }
        public class MemberInfo1DataVideo
        {
            public int view { get; set; }
        }
        public class MemberInfo1DataArticle
        {
            public int view { get; set; }
        }
        public class MemberInfo2
        {
            public int code { get; set; }
            public string message { get; set; }
            public MemberInfo2Data data { get; set; }
        }
        public class MemberInfo2Data
        {
            public int following { get; set; }
            public int follower { get; set; }
        }
        public class MemberInfo3
        {
            public int code { get; set; }
            public string message { get; set; }
            public MemberInfo3Data data { get; set; }
        }
        public class MemberInfo3Data
        {
            public string name { get; set; }
            public string sex { get; set; }
            public string sign { get; set; }
            public int level { get; set; }
        }
        public class Relation
        {
            public int code { get; set; }
            public string message { get; set; }
            public RelationData data { get; set; }
        }
        public class RelationData
        {
            public RelationDataRelation relation { get; set; }
            public RelationDataRelation be_relation { get; set; }
        }
        public class RelationDataRelation
        {
            public int mid { get; set; }
        }
    }
}
