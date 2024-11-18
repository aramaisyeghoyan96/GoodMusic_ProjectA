using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Models;
using DbContext;
using Microsoft.EntityFrameworkCore;
using Services;
using Microsoft.AspNetCore.Authorization;
using Npgsql.Replication;

namespace AppMusicRazor.Pages
{
    public class ListOfGroupsModel : PageModel
    {
        readonly IMusicService _service = null;
        readonly ILogger<ListOfGroupsModel> _logger = null;

        [BindProperty]
        public bool UseSeeds { get; set; } = true;
        
        public List<IMusicGroup> MusicGroups { get; set; }

        public int NrOfGroups { get; set; }

        //Pagination
        public int NrOfPages { get; set; }
        public int PageSize { get; } = 10;

        public int ThisPageNr { get; set; } = 0;
        public int PrevPageNr { get; set; } = 0;
        public int NextPageNr { get; set; } = 0;
        public int NrVisiblePages { get; set; } = 0;

        //ModelBinding for the form
        [BindProperty]
        public string SearchFilter { get; set; } = null;

        //will execute on a Get request
        public async Task<IActionResult> OnGet()
        {   
            //Read a QueryParameters
            if (int.TryParse(Request.Query["pagenr"], out int pagenr))
            {
                ThisPageNr = pagenr;
            }

            SearchFilter = Request.Query["search"];

            //Use the Service
            var resp = await _service.ReadMusicGroupsAsync(UseSeeds, false, SearchFilter, ThisPageNr, PageSize);
            MusicGroups = resp.PageItems;
            NrOfGroups = resp.DbItemsCount;

            //Pagination
            UpdatePagination(resp.DbItemsCount);

            return Page();
        }

        private void UpdatePagination(int nrOfItems)
        {
            //Pagination
            NrOfPages = (int)Math.Ceiling((double)nrOfItems / PageSize);
            PrevPageNr = Math.Max(0, ThisPageNr - 1);
            NextPageNr = Math.Min(NrOfPages - 1, ThisPageNr + 1);
            NrVisiblePages = Math.Min(10, NrOfPages);
        }

        public async Task<IActionResult> OnPostSearch()
        {
            //Use the Service
            var resp = await _service.ReadMusicGroupsAsync(UseSeeds, false, SearchFilter, ThisPageNr, PageSize);
            MusicGroups = resp.PageItems;
            NrOfGroups = resp.DbItemsCount;

            //Pagination
            UpdatePagination(resp.DbItemsCount);

            //Page is rendered as the postback is part of the form tag
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteGroup(Guid groupId)
        {
            await _service.DeleteMusicGroupAsync(groupId);

            //Use the Service
            var resp = await _service.ReadMusicGroupsAsync(UseSeeds, false, SearchFilter, ThisPageNr, PageSize);
            MusicGroups = resp.PageItems;
            NrOfGroups = resp.DbItemsCount;

            //Pagination
            UpdatePagination(resp.DbItemsCount);

            return Page();
        }

        //Inject services just like in WebApi
        public ListOfGroupsModel(IMusicService service, ILogger<ListOfGroupsModel> logger)
        {
            _service = service;
            _logger = logger;
        }
    }
}
