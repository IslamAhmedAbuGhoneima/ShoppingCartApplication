let dtble;

$(document).ready(function () {
    loadData();
});

// pure js
//document.addEventListener("DOMContentLoaded", () => {
//    loadData();
//})



function loadData() {
    dtble = $("#productsTable").DataTable({
        "ajax": {
            "url": "/Admin/product/GetProducts"
        },
        "columns": [
            { "data": "name" },
            { "data": "price" },
            { "data": "category.name" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <a href="/Admin/product/Edit/${data}" class="btn btn-success">Edit</a>
                        <button  onclick=deleteProduct("/Admin/product/DeleteProduct/${data}") class="btn btn-danger">Delete</button>
                    `
                }
            }
        ]
    });
}


function deleteProduct(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {

            fetch(url, { method:"Delete" })
                .then(
                    (resolve) => {
                        const result = resolve.json();
                        Swal.fire({
                            title: "Deleted!",
                            text: result.message,
                            icon: "success"
                        });
                        dtble.ajax.reload();
                    }
                )
        }
    });
}
