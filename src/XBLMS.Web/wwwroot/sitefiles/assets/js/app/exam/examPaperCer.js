var $url = "/exam/examPaperCer";

var data = utils.init({
  form: {
    keyWords: '',
    dateFrom: '',
    dateTo:''
  },
  list: [],
  cerDialogVisible:false
});

var methods = {
  apiGet: function() {
    var $this = this;

    if (this.total === 0) {
      utils.loading(this, true);
    }
 
    $api.get($url, { params: this.form }).then(function (response) {
      var res = response.data;
      $this.list = res.list;


    }).catch(function (error) {
      utils.error(error, { layer: true });
    }).then(function () {
      utils.loading($this, false);
    });
  },
  btnSearchClick: function () {
    this.cerDialogVisible = false;
    this.list = [];
    this.apiGet();
  },
  btnViewCer: function (cer) {
    top.utils.openLayerPhoto({
      title: cer.name,
      id: cer.id,
      src: cer.cerImg + '?r=' + Math.random()
    })
  },
};

var $vue = new Vue({
  el: "#main",
  data: data,
  methods: methods,
  created: function () {
    top.document.title = "我的证书";
    this.apiGet();
  },
});
