var Table;


$(document).ready(function() {
   loadDataTable();
});

function loadDataTable() {
    Table = $('#table1').DataTable({
        "ajax": {
            "url": "/Admin/Product/GetAll"
        },
        "columns": [
            { "data": "title", "width": "15%" },
            { "data": "isbn", "width": "15%" },
            { "data": "price", "width": "15%" },
            { "data": "author", "width": "15%" },
            { "data": "category.name", "width": "15%" },
            {
                "data": "imageUrl",
                "render": function (img) {



                    return `<div class="btn-group" role="group"> <img src="${img}" width="250" height="250"></div>`






                },
                "width": "15%"
            },

            {
                "data": "id",
                "render": function (data) {



                    return `<div class="btn-group" role="group"><a href="/Admin/Product/Upsert?id=${data}" class="btn btn-primary  mx-2">edit</a><a onClick=Delete('/Admin/Product/Delete/${data}') class="btn btn-danger mx-2">Delete</a>`






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