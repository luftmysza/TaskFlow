﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Domain.Entities;
public class UserUnreadComment
{
    public ApplicationUser User { get; set; }

    public Comment Comment { get; set; }

}
