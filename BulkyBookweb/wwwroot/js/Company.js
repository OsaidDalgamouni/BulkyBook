﻿var Table;


$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    Table = $('#table1').DataTable({
        "ajax": {
            "url": "/Admin/Company/GetAll"
        },
        "columns": [
            { "data": "name", "width": "15%" },
            { "data": "streetAddress", "width": "15%" },
            { "data": "city", "width": "15%" },
            { "data": "state", "width": "15%" },
            { "data": "phoneNumber", "width": "15%" },
           

            {
                "data": "id",
                "render": function (data) {



                    return `<div class="btn-group" role="group"><a href="/Admin/Company/Upsert?id=${data}" class="btn btn-primary  mx-2">edit</a><a onClick=Delete('/Admin/Company/Delete/${data}') class="btn btn-danger mx-2">Delete</a>`






                },
                "width": "15%"
            }



        ]
    });


}
function Delete(url) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'Delete',
                success: function (data) {
                    if (data.success) {
                        Table.ajax.reload();
                        toaster.success(data.message);
                    }
                    else {
                        toaster.error(data.message);
                    }
                }
            })
        }
    })

}