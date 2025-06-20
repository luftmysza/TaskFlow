using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Services;

public interface ICommentsSoapService
{
    public Task<List<Comment>> DumpCommentsAsync();
}
