using Imposter.Attributes.Checks;
using Imposter.services;
using Mummybot.Commands;
using Mummybot.Extentions;
using Qmmands;
using System.Threading.Tasks;

namespace Imposter.Commands.Modules
{
    [Group("code")]
    public class CodeModule : MummyModule
    {
        [Command("set")]
        public async Task SetCode([RequireCodeLenght(6)] string code)
        {
            if (CService.Code != "")
            {
                await ReplyAsync("a code it currently set, clear this code first!");
            }
            else
            {
                CService.Code = code;
                await Context.Message.AddOkAsync();
            }

        }
        public CodeService CService { get; set; }
        [Priority(2), Command]
        public async Task GetCode()
        {
            if (CService.Code == "")
                await ReplyAsync("I'm currently not aware of any Among us Room code.");
            else
                await ReplyAsync($"The current Among us Room code is: {CService.Code}.");
        }

        [Command("reset")]
        public async Task ResetCode()
        {
            CService.Code = "";
            await Context.Message.AddOkAsync();
        }


    }
}
