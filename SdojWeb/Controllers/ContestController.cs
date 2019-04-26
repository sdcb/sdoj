using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using Microsoft.AspNet.Identity;
using SdojWeb.Infrastructure.Alerts;
using SdojWeb.Infrastructure.Identity;
using SdojWeb.Manager;
using SdojWeb.Models;
using SdojWeb.Models.ContestModels;
using SdojWeb.Models.DbModels;

namespace SdojWeb.Controllers
{
    [SdojAuthorize]
    [RoutePrefix("contest")]
    public class ContestController : Controller
    {
        public ContestController(
            ContestManager manager,
            IPrincipal identity,
            ApplicationDbContext db)
        {
            _manager = manager;
            _identity = identity;
            _db = db;
        }

        public ActionResult Index()
        {
            bool isManager = _identity.IsInRole(SystemRoles.ContestAdmin);
            IQueryable<ContestListModel> data = _manager.List(GetCurrentUserId(), isManager);
            return View(data);
        }

        [SdojAuthorize(Roles = SystemRoles.ContestAdminOrCreator)]
        [HttpGet]
        public ActionResult Create()
        {
            return View(new ContestCreateModel());
        }

        [ValidateAntiForgeryToken, HttpPost, ActionName(nameof(Create))]
        [SdojAuthorize(Roles = SystemRoles.ContestAdminOrCreator)]
        public async Task<ActionResult> CreateConfirmed(ContestCreateModel model)
        {
            await model.Validate(_db, ModelState);
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            int id = await _manager.Create(model, GetCurrentUserId());
            return RedirectToAction(nameof(Details), new { id = id });
        }

        [Route("details/{contestId}")]
        public async Task<ActionResult> Details(int contestId)
        {
            if (!await HasAccess(contestId))
            {
                return RedirectToAction(nameof(Index)).WithWarning("此考试不存在或你无权限访问。");
            }
            ContestDetailsModel details = await _manager.Get(contestId);

            ViewBag.IsOwner = await IsOwner(contestId);
            return View(details);
        }

        [Route("details/{contestId}/start"), ValidateAntiForgeryToken]
        public async Task<ActionResult> StartContest(int contestId)
        {
            if (!await IsOwner(contestId))
            {
                return RedirectToAction(nameof(Details), new { contestId })
                    .WithWarning("只有拥有者才能控制该考试。");
            }

            await _manager.StartContest(contestId);
            return RedirectToAction(nameof(Details), new { contestId })
                .WithSuccess("已开始考试。");
        }

        [Route("details/{contestId}/complete"), ValidateAntiForgeryToken]
        public async Task<ActionResult> CompleteContest(int contestId)
        {
            if (!await IsOwner(contestId))
            {
                return RedirectToAction(nameof(Details), new { contestId })
                    .WithWarning("只有拥有者才能控制该考试。");
            }

            await _manager.CompleteContest(contestId);
            return RedirectToAction(nameof(Details), new { contestId })
                .WithSuccess("已结束考试。");
        }

        [Route("details/{contestId}/checkComplete"), ValidateAntiForgeryToken]
        public async Task<ActionResult> CheckComplete(int contestId)
        {
            if (!await HasAccess(contestId))
            {
                return RedirectToAction(nameof(Details), new { contestId })
                    .WithWarning("此考试不存在或你无权访问。");
            }

            await _manager.CheckComplete(contestId);
            return RedirectToAction(nameof(Details), new { contestId })
                .WithSuccess("已结束考试。");
        }

        [Route("details/{contestId}/question/{rank}")]
        public async Task<ActionResult> DetailsInQuestion(int contestId, int rank = 1)
        {
            if (!await HasAccess(contestId))
            {
                return RedirectToAction("Index").WithWarning("此考试不存在或你无权限访问。");
            }
            if (!await _manager.IsContestStarted(contestId))
            {
                return RedirectToAction(nameof(Details), new { contestId }).WithWarning("考试未开始或已结束。");
            }
            ContestDetailsModel details = await _manager.Get(contestId);
            ContestDetailsInQuestionModel vm = Mapper.Map<ContestDetailsInQuestionModel>(details);
            vm.CurrentQuestion = await _manager.GetQuestion(contestId, rank);
            vm.Rank = rank;
            return View(vm);
        }

        [Route("details/{contestId}/question-{questionId}/solutions")]
        public async Task<ActionResult> QuestionSolutions(int contestId, int questionId)
        {
            if (!await HasAccess(contestId, questionId))
            {
                return new HttpUnauthorizedResult();
            }

            List<SolutionSummaryModel> data = await _manager.GetQuestionSolutions(questionId);
            return PartialView(data);
        }

        [Route("details/{contestId}/question-{questionId}/submit")]
        [ValidateInput(false)]
        [HttpPost]
        public async Task<ActionResult> Submit(int contestId, int questionId, SolutionCreateModel model)
        {
            if (!ModelState.IsValid)
            {
                return new HttpStatusCodeResult(400, String.Join("\n", ModelState.Select(x => $"{x.Key}:{x.Value}")));
            }

            if (!await HasAccess(contestId))
            {
                return new HttpUnauthorizedResult();
            }

            if (!await _manager.IsContestStarted(contestId))
            {
                return new HttpStatusCodeResult(400, "考试未开始或已结束。");
            }

            int solutionId = await _manager.CreateSolution(contestId, model);
            await _manager.PushSolutionJudge(solutionId);
            return new HttpStatusCodeResult(201, solutionId.ToString());
        }

        private int GetCurrentUserId()
        {
            return _identity.Identity.GetUserId<int>();
        }

        private async Task<bool> IsOwner(int contestId)
        {
            return await _manager.IsOwner(
                contestId,
                User.IsInRole(SystemRoles.ContestAdmin),
                GetCurrentUserId());
        }

        private async Task<bool> HasAccess(int contestId)
        {
            return await _manager.HasAccess(
                contestId,
                User.IsInRole(SystemRoles.ContestAdmin),
                GetCurrentUserId());
        }

        private async Task<bool> HasAccess(int contestId, int questionId)
        {
            return await _manager.HasAccess(
                contestId, questionId, 
                User.IsInRole(SystemRoles.ContestAdmin),
                GetCurrentUserId());
        }

        private readonly ContestManager _manager;
        private readonly IPrincipal _identity;
        private readonly ApplicationDbContext _db;
    }
}
